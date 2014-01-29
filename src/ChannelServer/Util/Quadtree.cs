using System.Collections.Generic;
using System.Drawing;

namespace Aura.Channel.Util
{
	public interface IQuadtreeObject
	{
		/// <summary>
		/// The rectangle that defines the object's boundaries.
		/// </summary>
		Rectangle Rect { get; }
	}

	/// <summary>
	/// Simple, generic Quadtree.
	/// <remarks>
	/// Source: http://bytes.com/topic/c-sharp/insights/880968-generic-quadtree-implementation
	/// </remarks>
	/// </summary>
	/// <typeparam name="TObject"></typeparam>
	public class Quadtree<TObject> where TObject : IQuadtreeObject
	{
		/// <summary>
		/// Max amount of objects per node, before it's devided.
		/// </summary>
		private const int MaxObjects = 2;

		/// <summary>
		/// The area of this node.
		/// </summary>
		public Rectangle Rect { get; protected set; }

		public Quadtree<TObject> TopLeftChild { get; protected set; }
		public Quadtree<TObject> TopRightChild { get; protected set; }
		public Quadtree<TObject> BottomLeftChild { get; protected set; }
		public Quadtree<TObject> BottomRightChild { get; protected set; }

		/// <summary>
		/// Objects in *this* node (not the children).
		/// </summary>
		public List<TObject> Objects { get; protected set; }

		public Quadtree(Rectangle rect)
		{
			this.Rect = rect;
		}

		public Quadtree(int x, int y, int width, int height)
			: this(new Rectangle(x, y, width, height))
		{ }

		/// <summary>
		/// Adds object to node.
		/// </summary>
		/// <param name="item"></param>
		private void Add(TObject item)
		{
			if (this.Objects == null)
				this.Objects = new List<TObject>();

			this.Objects.Add(item);
		}

		/// <summary>
		/// Removes object from node.
		/// </summary>
		/// <param name="item"></param>
		private void Remove(TObject item)
		{
			if (this.Objects != null && this.Objects.Contains(item))
				this.Objects.Remove(item);
		}

		/// <summary>
		/// Divides node and moves children to appropriate ones where applicable.
		/// </summary>
		private void Split()
		{
			var size = new Point(this.Rect.Width / 2, this.Rect.Height / 2);
			var mid = new Point(this.Rect.X + size.X, this.Rect.Y + size.Y);

			this.TopLeftChild = new Quadtree<TObject>(new Rectangle(this.Rect.Left, this.Rect.Top, size.X, size.Y));
			this.TopRightChild = new Quadtree<TObject>(new Rectangle(mid.X, this.Rect.Top, size.X, size.Y));
			this.BottomLeftChild = new Quadtree<TObject>(new Rectangle(this.Rect.Left, mid.Y, size.X, size.Y));
			this.BottomRightChild = new Quadtree<TObject>(new Rectangle(mid.X, mid.Y, size.X, size.Y));

			for (int i = 0; i < this.Objects.Count; i++)
			{
				var destTree = this.GetDestinationTree(this.Objects[i]);

				if (destTree != this)
				{
					destTree.Insert(this.Objects[i]);
					this.Remove(this.Objects[i]);
					i--;
				}
			}
		}

		/// <summary>
		/// Get the child that would contain the object.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private Quadtree<TObject> GetDestinationTree(TObject item)
		{
			var destTree = this;

			if (this.TopLeftChild.Rect.Contains(item.Rect))
				destTree = this.TopLeftChild;
			else if (this.TopRightChild.Rect.Contains(item.Rect))
				destTree = this.TopRightChild;
			else if (this.BottomLeftChild.Rect.Contains(item.Rect))
				destTree = this.BottomLeftChild;
			else if (this.BottomRightChild.Rect.Contains(item.Rect))
				destTree = this.BottomRightChild;

			return destTree;
		}

		/// <summary>
		/// Clears the node and its children of all objects.
		/// </summary>
		public void Clear()
		{
			if (this.Objects != null)
			{
				this.Objects.Clear();
				this.Objects = null;
			}

			if (this.TopLeftChild != null)
			{
				this.TopLeftChild.Clear();
				this.TopRightChild.Clear();
				this.BottomLeftChild.Clear();
				this.BottomRightChild.Clear();

				this.TopLeftChild = null;
				this.TopRightChild = null;
				this.BottomLeftChild = null;
				this.BottomRightChild = null;
			}
		}

		/// <summary>
		/// Returns the amount of all objects in this node and its children.
		/// </summary>
		public int ObjectCount()
		{
			var count = 0;

			if (this.Objects != null)
				count += this.Objects.Count;

			if (this.TopLeftChild != null)
			{
				count += this.TopLeftChild.ObjectCount();
				count += this.TopRightChild.ObjectCount();
				count += this.BottomLeftChild.ObjectCount();
				count += this.BottomRightChild.ObjectCount();
			}

			return count;
		}

		/// <summary>
		/// Deletes object from node.
		/// </summary>
		/// <remarks>
		/// Children are removed as well, if object count reaches 0.
		/// </remarks>
		/// <param name="item"></param>
		public void Delete(TObject item)
		{
			bool objectRemoved = false;
			if (this.Objects != null && this.Objects.Contains(item))
			{
				this.Remove(item);
				objectRemoved = true;
			}

			if (this.TopLeftChild != null && !objectRemoved)
			{
				this.TopLeftChild.Delete(item);
				this.TopRightChild.Delete(item);
				this.BottomLeftChild.Delete(item);
				this.BottomRightChild.Delete(item);
			}

			if (this.TopLeftChild != null)
			{
				if (this.TopLeftChild.ObjectCount() == 0 && this.TopRightChild.ObjectCount() == 0 && this.BottomLeftChild.ObjectCount() == 0 && this.BottomRightChild.ObjectCount() == 0)
				{
					this.TopLeftChild = null;
					this.TopRightChild = null;
					this.BottomLeftChild = null;
					this.BottomRightChild = null;
				}
			}
		}

		/// <summary>
		/// Inserts item into node.
		/// </summary>
		/// <param name="item"></param>
		public void Insert(TObject item)
		{
			if (!this.Rect.IntersectsWith(item.Rect))
				return;

			if (this.Objects == null || (this.TopLeftChild == null && this.Objects.Count + 1 <= MaxObjects))
			{
				this.Add(item);
			}
			else
			{
				if (this.TopLeftChild == null)
					this.Split();

				var destTree = this.GetDestinationTree(item);
				if (destTree == this)
					this.Add(item);
				else
					destTree.Insert(item);
			}
		}

		/// <summary>
		/// Adds objects that intersect with rect to results.
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="results"></param>
		public void GetObjects(Rectangle rect, ref List<TObject> results)
		{
			if (results != null)
			{
				if (rect.Contains(this.Rect))
				{
					this.GetAllObjects(ref results);
				}
				else if (rect.IntersectsWith(this.Rect))
				{
					if (this.Objects != null)
					{
						for (int i = 0; i < this.Objects.Count; i++)
						{
							if (rect.IntersectsWith(this.Objects[i].Rect))
							{
								results.Add(this.Objects[i]);
							}
						}
					}

					if (this.TopLeftChild != null)
					{
						this.TopLeftChild.GetObjects(rect, ref results);
						this.TopRightChild.GetObjects(rect, ref results);
						this.BottomLeftChild.GetObjects(rect, ref results);
						this.BottomRightChild.GetObjects(rect, ref results);
					}
				}
			}
		}

		/// <summary>
		/// Adds all objects of this node and its children to results.
		/// </summary>
		/// <param name="results"></param>
		public void GetAllObjects(ref List<TObject> results)
		{
			if (this.Objects != null)
				results.AddRange(this.Objects);

			if (this.TopLeftChild != null)
			{
				this.TopLeftChild.GetAllObjects(ref results);
				this.TopRightChild.GetAllObjects(ref results);
				this.BottomLeftChild.GetAllObjects(ref results);
				this.BottomRightChild.GetAllObjects(ref results);
			}
		}
	}
}
