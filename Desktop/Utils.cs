using RemoteController.Win32.Desktop;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace RemoteController.Desktop
{
    public static class Utils
    {
        #region ImageSource to Icon

        /// <summary>
        /// Reads a given image resource into a WinForms icon.
        /// </summary>
        /// <param name="imageSource">Image source pointing to
        /// an icon file (*.ico).</param>
        /// <returns>An icon object that can be used with the
        /// taskbar area.</returns>
        public static Icon ToIcon(this ImageSource imageSource)
        {
            if (imageSource == null)
                return null;
            if (imageSource is DrawingImage)
                return FromDrawImage((DrawingImage)imageSource);

            Uri uri = new Uri(imageSource.ToString());
            StreamResourceInfo streamInfo = Application.GetResourceStream(uri);

            if (streamInfo == null)
            {
                string msg = "The supplied image source '{0}' could not be resolved.";
                msg = string.Format(msg, imageSource);
                throw new ArgumentException(msg);
            }

            return new Icon(streamInfo.Stream);
        }

        private static Icon FromDrawImage(DrawingImage source)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image
            {
                Source = source,
                Stretch = Stretch.Uniform
            };
            image.Arrange(new Rect(0, 0, 32, 32));
            image.UpdateLayout();
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(image);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                encoder.Save(stream);

                Bitmap bmp = new Bitmap(stream);
                IntPtr Hicon = bmp.GetHicon();
                Icon newIcon = Icon.FromHandle(Hicon);

                return newIcon;
            }
        }

        #endregion

        #region evaluate listings

        /// <summary>
        /// Checks a list of candidates for equality to a given
        /// reference value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The evaluated value.</param>
        /// <param name="candidates">A liste of possible values that are
        /// regarded valid.</param>
        /// <returns>True if one of the submitted <paramref name="candidates"/>
        /// matches the evaluated value. If the <paramref name="candidates"/>
        /// parameter itself is null, too, the method returns false as well,
        /// which allows to check with null values, too.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="candidates"/>
        /// is a null reference.</exception>
        public static bool Is<T>(this T value, params T[] candidates)
        {
            if (candidates == null)
                return false;

            foreach (T t in candidates)
            {
                if (value.Equals(t))
                    return true;
            }

            return false;
        }

        #endregion

        #region match MouseEvent to PopupActivation

        /// <summary>
        /// Checks if a given <see cref="PopupActivationMode"/> is a match for
        /// an effectively pressed mouse button.
        /// </summary>
        public static bool IsMatch(this MouseEvent me, PopupActivationMode activationMode)
        {
            switch (activationMode)
            {
                case PopupActivationMode.LeftClick:
                    return me == MouseEvent.IconLeftMouseUp;
                case PopupActivationMode.RightClick:
                    return me == MouseEvent.IconRightMouseUp;
                case PopupActivationMode.LeftOrRightClick:
                    return me.Is(MouseEvent.IconLeftMouseUp, MouseEvent.IconRightMouseUp);
                case PopupActivationMode.LeftOrDoubleClick:
                    return me.Is(MouseEvent.IconLeftMouseUp, MouseEvent.IconDoubleClick);
                case PopupActivationMode.DoubleClick:
                    return me.Is(MouseEvent.IconDoubleClick);
                case PopupActivationMode.MiddleClick:
                    return me == MouseEvent.IconMiddleMouseUp;
                case PopupActivationMode.All:
                    //return true for everything except mouse movements
                    return me != MouseEvent.MouseMove;
                default:
                    throw new ArgumentOutOfRangeException("activationMode");
            }
        }

        #endregion
    }
}
