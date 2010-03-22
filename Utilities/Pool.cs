using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace Utility
{
  public abstract class PooledObject
  {
    internal bool valid;
    internal int index;

    protected virtual void OnRelease() { }

    public void Release()
    {
      OnRelease();
      valid = false;
      Pool.pools[GetType()].Release( this );
    }
  }

  public abstract class Pool
  {
    internal static Dictionary<Type, Pool> pools = new Dictionary<Type,Pool>( 20 );
    protected abstract Type ObjectType { get; }

    protected Pool()
    {
      pools.Add( this.ObjectType, this );
    }

    internal abstract void Release( PooledObject obj );

    protected abstract void CleanUp();

    public static void CleanupAll()
    {
      foreach ( var kvp in pools )
        kvp.Value.CleanUp();
    }
  }

  /// <summary>
  /// A collection that maintains a set of class instances to allow for recycling
  /// instances and minimizing the effects of garbage collection.
  /// </summary>
  /// <typeparam name="T">The type of object to store in the Pool. Pools can only hold class types.</typeparam>
  public class Pool<T> : Pool where T : PooledObject
  {
    // the actual items of the pool
    private T[] items;
    internal List<int> releasedItems;

    public T[] Items { get { return items; } }

    // used for allocating instances of the object
    private readonly Allocate allocate;

    // a constructor the default allocate method can use to create instances
    private readonly ConstructorInfo constructor;

    protected override Type ObjectType { get { return typeof( T ); } }

    /// <summary>
    /// Gets or sets a delegate used for initializing objects before returning them from the New method.
    /// </summary>
    public Action<T> Initialize { get; set; }

    /// <summary>
    /// Gets or sets a delegate that is run when an object is moved from being valid to invalid
    /// in the CleanUp method.
    /// </summary>
    public Action<T> Deinitialize { get; set; }

    /// <summary>
    /// Gets the number of valid objects in the pool.
    /// </summary>
    public int ValidCount { get { return items.Length - invalidCount; } }

    /// <summary>
    /// Gets the number of invalid objects in the pool.
    /// </summary>
    int invalidCount;
    public int InvalidCount { get { return invalidCount; } }

    /// <summary>
    /// Returns a valid object at the given index. The index must fall in the range of [0, ValidCount].
    /// </summary>
    /// <param name="index">The index of the valid object to get</param>
    /// <returns>A valid object found at the index</returns>
    public T this[int index]
    {
      get
      {
        index += invalidCount;

        if ( index < invalidCount || index >= items.Length )
          throw new IndexOutOfRangeException( "The index must be less than or equal to ValidCount" );

        return items[index];
      }
    }

    /// <summary>
    /// Creates a new pool.
    /// </summary>
    /// <param name="validateFunc">A predicate used to determine if a given object is still valid.</param>
    public Pool() : this( 0 ) { }

    /// <summary>
    /// Creates a new pool with a specific starting size.
    /// </summary>
    /// <param name="initialSize">The initial size of the pool.</param>
    /// <param name="validateFunc">A predicate used to determine if a given object is still valid.</param>
    public Pool( int initialSize ) : this( initialSize, null ) { }

    /// <summary>
    /// Creates a new pool with a specific starting size.
    /// </summary>
    /// <param name="initialSize">The initial size of the pool.</param>
    /// <param name="validateFunc">A predicate used to determine if a given object is still valid.</param>
    /// <param name="allocateFunc">A function used to allocate an instance for the pool.</param>
    public Pool( int initialSize, Allocate allocateFunc )
    {
      // validate some parameters
      if ( initialSize < 0 )
        throw new ArgumentException( "initialSize must be non-negative" );

      if ( initialSize == 0 )
        initialSize = 10;

      items = new T[initialSize];
      invalidCount = items.Length;

      releasedItems = new List<int>( initialSize );

      // default to using a parameterless constructor if no allocateFunc was given
      allocate = allocateFunc ?? ConstructorAllocate;

      // if we are using the ConstructorAllocate method, make sure we have a valid parameterless constructor
      if ( allocate == ConstructorAllocate )
      {
        // we want to find any parameterless constructor, public or not
        constructor = typeof( T ).GetConstructor(
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
          null,
          new Type[] { },
          null );

        if ( constructor == null )
          throw new InvalidOperationException( typeof( T ) + " does not have a parameterless constructor." );
      }
    }

    /// <summary>
    /// Cleans up the pool by checking each valid object to ensure it is still actually valid.
    /// </summary>
    protected override void CleanUp()
    {
      if ( releasedItems.Count != 0 )
      {
        foreach ( int i in releasedItems )
          Release( items[i] );
        releasedItems.Clear();
      }
    }

    /// <summary>
    /// Returns a new object from the Pool.
    /// </summary>
    /// <returns>The next object in the pool if available, null if all instances are valid.</returns>
    public T New()
    {
      // if we're out of invalid instances, resize to fit some more
      if ( invalidCount == 0 )
      {
        int resizeAmount = items.Length;
#if DEBUG
        Trace.WriteLine( "Resizing pool. Old size: " + items.Length + ". New size: " + ( items.Length + resizeAmount ) );
#endif
        // create a new array with some more slots and copy over the existing items
        T[] newItems = new T[items.Length + resizeAmount];
        for ( int i = items.Length - 1; i >= 0; i-- )
          newItems[i + resizeAmount] = items[i];
        items = newItems;

        // move the invalid count based on our resize amount
        invalidCount += resizeAmount;
      }

      // decrement the counter
      invalidCount--;

      // get the next item in the list
      T obj = items[invalidCount];

      // if the item is null, we need to allocate a new instance
      if ( obj == null )
      {
        obj = allocate();

        if ( obj == null )
          throw new InvalidOperationException( "The pool's allocate method returned a null object reference." );

        items[invalidCount] = obj;
      }

      obj.index = invalidCount;
      obj.valid = true;

      // initialize the object if a delegate was provided
      if ( Initialize != null )
        Initialize( obj );

      return obj;
    }

    // a default Allocate delegate for use when no custom allocate delegate is provided
    private T ConstructorAllocate()
    {
      return constructor.Invoke( null ) as T;
    }

    /// <summary>
    /// A delegate that returns a new object instance for the Pool.
    /// </summary>
    /// <returns>A new object instance.</returns>
    public delegate T Allocate();

    internal override void Release( PooledObject obj )
    {
      PooledObject temp = items[obj.index];
      items[obj.index] = items[invalidCount];
      items[invalidCount++] = (T)temp;
    }
  }
}