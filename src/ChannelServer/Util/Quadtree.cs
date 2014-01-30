// https://gist.github.com/ismyhc/4747262

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace Aura.Channel.Util
{
	public interface IQuadtreeObject
	{
		Rectangle Rect { get; }
	}

	public class Quadtree<T> where T : IQuadtreeObject
	{
		private int MaxObjects = 2;
		private int MaxLevels = 5;

		private int _level;
		private List<T> _objects;
		private Rectangle _bounds;
		private Quadtree<T>[] _nodes;

		public Quadtree(int x, int y, int width, int height)
			: this(0, new Rectangle(x, y, width, height))
		{
		}

		public Quadtree(int level, Rectangle bounds)
		{
			_level = level;
			_bounds = bounds;
			_objects = new List<T>();
			_nodes = new Quadtree<T>[4];
		}

		public void Clear()
		{
			_objects.Clear();
			for (int i = 0; i < _nodes.Length; i++)
			{
				if (_nodes[i] != null)
				{
					_nodes[i].Clear();
					_nodes[i] = null;
				}
			}
		}

		private void Split()
		{
			int subWidth = (int)(_bounds.Width / 2);
			int subHeight = (int)(_bounds.Height / 2);
			int x = (int)_bounds.X;
			int y = (int)_bounds.Y;
			_nodes[0] = new Quadtree<T>(_level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight));
			_nodes[1] = new Quadtree<T>(_level + 1, new Rectangle(x, y, subWidth, subHeight));
			_nodes[2] = new Quadtree<T>(_level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight));
			_nodes[3] = new Quadtree<T>(_level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight));
		}

		private List<int> GetIndexes(Rectangle rect)
		{
			var indexes = new List<int>();
			double verticalMidpoint = _bounds.X + (_bounds.Width / 2);
			double horizontalMidpoint = _bounds.Y + (_bounds.Height / 2);
			bool topQuadrant = rect.Y >= horizontalMidpoint;
			bool bottomQuadrant = (rect.Y - rect.Height) <= horizontalMidpoint;
			bool topAndBottomQuadrant = rect.Y + rect.Height + 1 >= horizontalMidpoint && rect.Y + 1 <= horizontalMidpoint;
			if (topAndBottomQuadrant)
			{
				topQuadrant = false;
				bottomQuadrant = false;
			}

			if (rect.X + rect.Width + 1 >= verticalMidpoint && rect.X - 1 <= verticalMidpoint)
			{
				if (topQuadrant)
				{
					indexes.Add(2);
					indexes.Add(3);
				}
				else if (bottomQuadrant)
				{
					indexes.Add(0);
					indexes.Add(1);
				}
				else if (topAndBottomQuadrant)
				{
					indexes.Add(0);
					indexes.Add(1);
					indexes.Add(2);
					indexes.Add(3);
				}
			}
			else if (rect.X + 1 >= verticalMidpoint)
			{
				if (topQuadrant)
				{
					indexes.Add(3);
				}
				else if (bottomQuadrant)
				{
					indexes.Add(0);
				}
				else if (topAndBottomQuadrant)
				{
					indexes.Add(3);
					indexes.Add(0);
				}
			}
			else if (rect.X - rect.Width <= verticalMidpoint)
			{
				if (topQuadrant)
				{
					indexes.Add(2);
				}
				else if (bottomQuadrant)
				{
					indexes.Add(1);
				}
				else if (topAndBottomQuadrant)
				{
					indexes.Add(2);
					indexes.Add(1);
				}
			}
			else
			{
				indexes.Add(-1);
			}
			return indexes;
		}

		public void Insert(T obj)
		{
			Rectangle pRect = obj.Rect;
			if (_nodes[0] != null)
			{
				var indexes = this.GetIndexes(pRect);
				for (int ii = 0; ii < indexes.Count; ii++)
				{
					int index = indexes[ii];
					if (index != -1)
					{
						_nodes[index].Insert(obj);
						return;
					}
				}

			}
			_objects.Add(obj);
			if (_objects.Count > MaxObjects && _level < MaxLevels)
			{
				if (_nodes[0] == null)
					this.Split();

				int i = 0;
				while (i < _objects.Count)
				{
					var sqaureOne = _objects[i];
					var oRect = sqaureOne.Rect;
					var indexes = this.GetIndexes(oRect);
					for (int k = 0; k < indexes.Count; k++)
					{
						int index = indexes[k];
						if (index != -1)
						{
							_nodes[index].Insert(sqaureOne);
							_objects.Remove(sqaureOne);
						}
						else
						{
							i++;
						}
					}
				}
			}
		}

		public List<T> Get(Rectangle rect)
		{
			var result = new List<T>();
			this.Retrieve(rect, ref result);
			return result;
		}

		public void Retrieve(Rectangle rect, ref List<T> result)
		{
			var indexes = GetIndexes(rect);

			for (int i = 0; i < indexes.Count; i++)
			{
				int index = indexes[i];
				if (index != -1 && _nodes[0] != null)
				{
					_nodes[index].Retrieve(rect, ref result);
				}
				result.AddRange(_objects);
			}
		}
	}
}
