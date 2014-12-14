// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Aura.Channel.Scripting.Scripts
{
	public class DialogElement
	{
		public static implicit operator DialogElement(string msg)
		{
			return (DialogText)msg;
		}

		public List<DialogElement> Children { get; protected set; }

		public DialogElement()
		{
			this.Children = new List<DialogElement>();
		}

		public DialogElement(params DialogElement[] elements)
			: this()
		{
			this.Children.AddRange(elements);
		}

		public DialogElement Add(params DialogElement[] elements)
		{
			this.Children.AddRange(elements);
			return this;
		}

		public virtual void Render(ref StringBuilder sb)
		{
			foreach (var child in Children)
				child.Render(ref sb);
		}

		/// <summary>
		/// Renders this and its child elements.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var sb = new StringBuilder();
			this.Render(ref sb);
			return sb.ToString();
		}
	}

	/// <summary>
	/// Simple text. Strings passed to Msg are converted into this.
	/// </summary>
	public class DialogText : DialogElement
	{
		public string Text { get; set; }

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.String"/> to <see cref="DialogText"/>.
		/// </summary>
		/// <param name="msg">The msg.</param>
		/// <returns>
		/// A new DialogText instance with the string as the text.
		/// </returns>
		public static implicit operator DialogText(string msg)
		{
			return new DialogText(msg);
		}

		public DialogText(string format, params object[] args)
		{
			this.Text = string.Format(format, args);
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.Append(this.Text);

			base.Render(ref sb);
		}
	}

	/// <summary>
	/// Changes the NPC portrait, displayed at the upper left of the dialog.
	/// </summary>
	public class DialogPortrait : DialogElement
	{
		public string Text { get; set; }

		public DialogPortrait(string text)
		{
			if (text == null)
				this.Text = "NONE";
			else
				this.Text = text;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<npcportrait name='{0}' />", this.Text);

			base.Render(ref sb);
		}
	}

	/// <summary>
	/// Changes the name of the speaking person (at the top of the dialog).
	/// </summary>
	public class DialogTitle : DialogElement
	{
		public string Text { get; set; }

		public DialogTitle(string text)
		{
			if (text == null)
				this.Text = "NONE";
			else
				this.Text = text;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<title name='{0}' />", this.Text);

			base.Render(ref sb);
		}
	}

	/// <summary>
	/// Shows the configured hotkey for the given id.
	/// </summary>
	public class DialogHotkey : DialogElement
	{
		public string Text { get; set; }

		public DialogHotkey(string text)
		{
			this.Text = text;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<hotkey name='{0}' />", this.Text);

			base.Render(ref sb);
		}
	}

	/// <summary>
	/// A button is displayed at the bottom of the dialog, and can be clicked.
	/// The keyword of the button is sent to the server and can be read using Select.
	/// </summary>
	public class DialogButton : DialogElement
	{
		public string Text { get; set; }
		public string Keyword { get; set; }
		public string OnFrame { get; set; }

		public DialogButton(string text, string keyword = null, string onFrame = null)
		{
			this.Text = text;
			this.OnFrame = onFrame;

			if (keyword != null)
				this.Keyword = keyword;
			else
			{
				// Take text, prepend @, replace all non-numerics with _ and
				// make the string lower case, if no keyword was given.
				// Yea... hey, this is 10 times faster than Regex + ToLower!
				var sb = new StringBuilder();
				sb.Append('@');
				foreach (var c in text)
				{
					if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z'))
						sb.Append(c);
					else if (c >= 'A' && c <= 'Z')
						sb.Append((char)(c + 32));
					else
						sb.Append('_');
				}
				this.Keyword = sb.ToString();
			}
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<button title='{0}' keyword='{1}'", this.Text, this.Keyword);
			if (this.OnFrame != null)
				sb.AppendFormat(" onframe='{0}'", this.OnFrame);
			sb.Append(" />");
		}
	}

	/// <summary>
	/// Changes the background music, for the duration of the dialog.
	/// </summary>
	public class DialogBgm : DialogElement
	{
		public string File { get; set; }

		public DialogBgm(string file)
		{
			this.File = file;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<music name='{0}'/>", this.File);
		}
	}

	/// <summary>
	/// Shows an image in the center of the screen.
	/// </summary>
	public class DialogImage : DialogElement
	{
		public string File { get; set; }
		public bool Localize { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public DialogImage(string name, bool localize = false, int width = 0, int height = 0)
		{
			this.File = name;
			this.Localize = localize;
			this.Width = width;
			this.Height = height;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<image name='{0}'", this.File);
			if (this.Localize)
				sb.Append(" local='true'");
			if (this.Width != 0)
				sb.AppendFormat(" width='{0}'", this.Width);
			if (this.Height != 0)
				sb.AppendFormat(" height='{0}'", this.Height);

			sb.Append(" />");
		}
	}

	/// <summary>
	/// Displays a list of options (buttons) in a separate window.
	/// Result is sent like a regular button click.
	/// </summary>
	public class DialogList : DialogElement
	{
		public string Text { get; set; }
		public string CancelKeyword { get; set; }
		public int Height { get; set; }

		public DialogList(string text, int height, string cancelKeyword, params DialogButton[] elements)
		{
			this.Height = height;
			this.Text = text;
			this.CancelKeyword = cancelKeyword;
			this.Add(elements);
		}

		public DialogList(string text, params DialogButton[] elements)
			: this(text, (int)elements.Length, elements)
		{ }

		public DialogList(string text, int height, params DialogButton[] elements)
			: this(text, height, "@end", elements)
		{ }

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<listbox page_size='{0}' title='{1}' cancel='{2}'>", this.Height, this.Text, this.CancelKeyword);
			base.Render(ref sb);
			sb.Append("</listbox>");
		}
	}

	/// <summary>
	/// Shows a single lined input box. The result is sent as a regular
	/// Select result.
	/// </summary>
	public class DialogInput : DialogElement
	{
		public string Title { get; set; }
		public string Text { get; set; }
		public byte MaxLength { get; set; }
		public bool Cancelable { get; set; }

		public DialogInput(string title = "Input", string text = "", byte maxLength = 20, bool cancelable = true)
		{
			this.Title = title;
			this.Text = text;
			this.MaxLength = maxLength;
			this.Cancelable = cancelable;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<inputbox title='{0}' caption='{1}' max_len='{2}' allow_cancel='{3}' />", this.Title, this.Text, this.MaxLength, this.Cancelable);

			base.Render(ref sb);
		}
	}

	/// <summary>
	/// Dialog automatically continues after x ms.
	/// </summary>
	public class DialogAutoContinue : DialogElement
	{
		public int Duration { get; set; }

		public DialogAutoContinue(int duration)
		{
			this.Duration = duration;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<autopass duration='{0}'/>", this.Duration);
		}
	}

	/// <summary>
	/// Changes the facial expression of the portrait.
	/// (Defined client sided in the db/npc/npcportrait_ani_* files.)
	/// </summary>
	public class DialogFaceExpression : DialogElement
	{
		public string Expression { get; set; }

		public DialogFaceExpression(string expression)
		{
			this.Expression = expression;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<face name='{0}'/>", this.Expression);
		}
	}

	/// <summary>
	/// Plays a movie in a box in the center of the screen.
	/// Files are taken from movie/.
	/// </summary>
	public class DialogMovie : DialogElement
	{
		public string File { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public bool Loop { get; set; }

		public DialogMovie(string file, int width, int height, bool loop = true)
		{
			this.File = file;
			this.Width = width;
			this.Height = height;
			this.Loop = loop;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<movie name='{0}' width='{1}' height='{2}' loop='{3}' />", this.File, this.Width, this.Height, this.Loop);
		}
	}

	/// <summary>
	/// Opens minimap, which is usually hidden during conversations.
	/// </summary>
	public class DialogMinimap : DialogElement
	{
		public bool Zoom { get; set; }
		public bool MaxSize { get; set; }
		public bool Center { get; set; }

		public DialogMinimap(bool zoom, bool maxSize, bool center)
		{
			this.Zoom = zoom;
			this.MaxSize = maxSize;
			this.Center = center;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<openminimap zoom='{0}' max_size='{1}' center='{2}' />", this.Zoom, this.MaxSize, this.Center);
		}
	}

	/// <summary>
	/// Displays marker on minimap for specified duration.
	/// </summary>
	public class DialogShowPosition : DialogElement
	{
		public int Region { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int RemainingTime { get; set; }

		public DialogShowPosition(int region, int x, int y, int remainingTime)
		{
			this.Region = region;
			this.X = x;
			this.Y = y;
			this.RemainingTime = remainingTime;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<show_position region='{0}' pos='{1} {2}' remainingtime='{3}' />", this.Region, this.X, this.Y, this.RemainingTime);
		}
	}

	/// <summary>
	/// Turns camera into the direction of the position.
	/// </summary>
	public class DialogShowDirection : DialogElement
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Angle { get; set; }

		public DialogShowDirection(int x, int y, int angle)
		{
			this.X = x;
			this.Y = y;
			this.Angle = angle;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<show_dir pos='{0} {1}' pitch='{2}' />", this.X, this.Y, this.Angle);
		}
	}

	/// <summary>
	/// Changes the name displayed for the NPC for the rest of the conversation.
	/// </summary>
	public class DialogSetDefaultName : DialogElement
	{
		public string Name { get; set; }

		public DialogSetDefaultName(string name)
		{
			this.Name = name;
		}

		public override void Render(ref StringBuilder sb)
		{
			sb.AppendFormat("<defaultname name='{0}' />", this.Name);
		}
	}
}
