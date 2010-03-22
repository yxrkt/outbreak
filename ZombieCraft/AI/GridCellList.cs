using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZombieCraft
{
  class GridCellListNode
  {
    public readonly int EntityIndex;
    internal readonly GridCellListNode[] next;
    internal readonly GridCellListNode[] prev;
    internal readonly GridCellList[] parent;

    public GridCellListNode( int entityIndex )
    {
      EntityIndex = entityIndex;
      next = new GridCellListNode[4];
      prev = new GridCellListNode[4];
      parent = new GridCellList[4];
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

  class GridCellList
  {
    readonly int cellType;

    GridCellListNode head;
    GridCellListNode tail;

    int count;
    public int Count { get { return count; } }

    public GridCellList( int row, int col )
    {
      cellType = ( ( ( row & 1 ) << 1 ) | ( col & 1 ) );
    }

    public void Add( GridCellListNode node )
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
      }

      count++;
    }

    internal void Remove( GridCellListNode node )
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

      count--;
    }

    public void Remove( int entityIndex )
    {
      for ( GridCellListNode node = head; node != null; node = node.next[cellType] )
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

    public GridCellListNode First()
    {
      return head;
    }

    public GridCellListNode Last()
    {
      return tail;
    }

    public GridCellListNode Next( GridCellListNode node )
    {
      return node.next[cellType];
    }
  }
}