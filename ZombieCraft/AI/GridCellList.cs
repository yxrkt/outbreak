using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZombieCraft
{
  class EntityListNode
  {
    public readonly int EntityIndex;
    internal readonly EntityListNode[] next;
    internal readonly EntityListNode[] prev;
    internal readonly EntityList[] parent;
    public int debugFrame;

    public EntityListNode( int entityIndex )
    {
      EntityIndex = entityIndex;
      next = new EntityListNode[4];
      prev = new EntityListNode[4];
      parent = new EntityList[4];
      debugFrame = 0;
    }

    public void Abandon()
    {
      for ( int i = 0; i < 4; ++i )
      {
        if ( parent[i] != null )
          parent[i].Remove( this );
      }
    }
  }

  class EntityList
  {
    public readonly int cellType;

    EntityListNode head;
    EntityListNode tail;

    int count;
    public int Count { get { return count; } }

    public EntityList( int row, int col )
    {
      cellType = ( ( ( row & 1 ) << 1 ) | ( col & 1 ) );
    }

    public void Add( EntityListNode node )
    {
#if DEBUG
      if ( node.parent[cellType] != null )
        throw new InvalidOperationException( "Node is already in a list." );
      if ( node.prev[cellType] != null || node.next[cellType] != null )
        throw new InvalidOperationException( "Node appears to belong to another list of the same type." );
#endif
      node.parent[cellType] = this;
      node.prev[cellType] = tail;

      if ( head == null )
      {
        head = node;
        tail = node;
      }
      else
      {
        tail.next[cellType] = node;
        tail = node;
      }

      count++;
    }

    internal void Remove( EntityListNode node )
    {
      if ( node == head )
        head = node.next[cellType];
      if ( node == tail )
        tail = node.prev[cellType];

      if ( node.prev[cellType] != null )
        node.prev[cellType].next[cellType] = node.next[cellType];
      if ( node.next[cellType] != null )
        node.next[cellType].prev[cellType] = node.prev[cellType];

      node.prev[cellType] = null;
      node.next[cellType] = null;

      node.parent[cellType] = null;

      count--;
    }

    public void Remove( int entityIndex )
    {
      for ( EntityListNode node = head; node != null; node = node.next[cellType] )
      {
        if ( node.EntityIndex == entityIndex )
        {
          Remove( node );
          break;
        }
      }
    }

    public void Clear()
    {
      while ( head != null )
        Remove( head );
    }

    public EntityListNode First()
    {
      return head;
    }

    public EntityListNode Last()
    {
      return tail;
    }

    public EntityListNode Next( EntityListNode node )
    {
      return node.next[cellType];
    }
  }
}