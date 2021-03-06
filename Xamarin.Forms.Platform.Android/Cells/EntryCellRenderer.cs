using System.ComponentModel;
using Android.Content;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class EntryCellRenderer : CellRenderer
	{
		EntryCellView _view;

		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
		{
			if ((_view = convertView as EntryCellView) == null)
				_view = new EntryCellView(context, item);
			else
			{
				_view.TextChanged = null;
				_view.FocusChanged = null;
				_view.EditingCompleted = null;
			}

			UpdateLabel();
			UpdateLabelColor();
			UpdatePlaceholder();
			UpdateKeyboard();
			UpdateHorizontalTextAlignment();
			UpdateText();
			UpdateIsEnabled();
			UpdateHeight();

			_view.TextChanged = OnTextChanged;
			_view.EditingCompleted = OnEditingCompleted;

			return _view;
		}

		protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnCellPropertyChanged(sender, e);

			if (e.PropertyName == EntryCell.LabelProperty.PropertyName)
				UpdateLabel();
			else if (e.PropertyName == EntryCell.TextProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == EntryCell.PlaceholderProperty.PropertyName)
				UpdatePlaceholder();
			else if (e.PropertyName == EntryCell.KeyboardProperty.PropertyName)
				UpdateKeyboard();
			else if (e.PropertyName == EntryCell.LabelColorProperty.PropertyName)
				UpdateLabelColor();
			else if (e.PropertyName == EntryCell.HorizontalTextAlignmentProperty.PropertyName)
				UpdateHorizontalTextAlignment();
			else if (e.PropertyName == Cell.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == "RenderHeight")
				UpdateHeight();
		}

		void OnEditingCompleted()
		{
			var entryCell = (IEntryCellController)Cell;
			entryCell.SendCompleted();
		}

		void OnTextChanged(string text)
		{
			var entryCell = (EntryCell)Cell;
			entryCell.Text = text;
		}

		void UpdateHeight()
		{
			_view.SetRenderHeight(Cell.RenderHeight);
		}

		void UpdateHorizontalTextAlignment()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.Gravity = entryCell.HorizontalTextAlignment.ToHorizontalGravityFlags();
		}

		void UpdateIsEnabled()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.Enabled = entryCell.IsEnabled;
		}

		void UpdateKeyboard()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.InputType = entryCell.Keyboard.ToInputType();
		}

		void UpdateLabel()
		{
			_view.LabelText = ((EntryCell)Cell).Label;
		}

		void UpdateLabelColor()
		{
			_view.SetLabelTextColor(((EntryCell)Cell).LabelColor, global::Android.Resource.Color.PrimaryTextDark);
		}

		void UpdatePlaceholder()
		{
			var entryCell = (EntryCell)Cell;
			_view.EditText.Hint = entryCell.Placeholder;
		}

		void UpdateText()
		{
			var entryCell = (EntryCell)Cell;
			if (_view.EditText.Text == entryCell.Text)
				return;

			_view.EditText.Text = entryCell.Text;
		}
	}
}