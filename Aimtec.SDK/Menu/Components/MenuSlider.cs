﻿namespace Aimtec.SDK.Menu.Components
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;

    using Aimtec.SDK.Menu.Theme;
    using Aimtec.SDK.Menu.Theme.Default;
    using Aimtec.SDK.Util;

    using Newtonsoft.Json;

    /// <summary>
    ///     Class MenuSlider. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.MenuComponent" />
    /// <seealso cref="int" />
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MenuSlider : MenuComponent, IReturns<int>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuSlider" /> class.
        /// </summary>
        /// <param name="internalName">The internal name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="value">The value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="shared">Whether this item is shared across instances</param>
        public MenuSlider(string internalName, string displayName, int value, int minValue = 0, int maxValue = 100, bool shared = false)
        {
            this.InternalName = internalName;
            this.DisplayName = displayName;
            this.Shared = shared;

            this.Value = value;
            this.MinValue = minValue;
            this.MaxValue = maxValue;

            if (this.Value > this.MaxValue)
            {
                Logger.Warn($"The value for slider {this.InternalName} is greater than the maximum value of the slider. Setting to maximum.");
                this.Value = maxValue;
            }

            else if (this.Value < this.MinValue)
            {
                Logger.Warn($"The value for slider {this.InternalName} is lower than the minimum value of the slider. Setting to minimum.");
                this.Value = minValue;
            }

            if (this.MinValue > this.MaxValue)
            {
                Logger.Error($"The minimum value is greater than the maximum value for slider with name \"{internalName}\"");
                throw new ArgumentException("The minimum value cannot be greater than the maximum value. Item name: {internalName}");
            }
        }

        [JsonConstructor]
        private MenuSlider()
        {

        }

        #endregion

        #region Public Properties

        internal override string Serialized => JsonConvert.SerializeObject(this, Formatting.Indented);



        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [JsonProperty(Order = 3)]
        public new int Value { get; set; }
    
        /// <summary>
        ///     Gets or sets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        public int MaxValue { get; set; }

        /// <summary>
        ///     Gets or sets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        public int MinValue { get; set; }



        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether [mouse down].
        /// </summary>
        /// <value><c>true</c> if [mouse down]; otherwise, <c>false</c>.</value>
        private bool MouseDown { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the render manager.
        /// </summary>
        /// <returns>Aimtec.SDK.Menu.Theme.IRenderManager.</returns>
        public override IRenderManager GetRenderManager()
        {
            return MenuManager.Instance.Theme.BuildMenuSliderRenderer(this);
        }

        public override Rectangle GetBounds(Vector2 pos)
        {
            return MenuManager.Instance.Theme.GetMenuSliderControlBounds(pos, this.Parent.Width);
        }


        /// <summary>
        ///     An application-defined function that processes messages sent to a window.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wparam">Additional message information.</param>
        /// <param name="lparam">Additional message information.</param>
        public override void WndProc(uint message, uint wparam, int lparam)
        {
            if ((message == (ulong) WindowsMessages.WM_LBUTTONDOWN
                || message == (ulong) WindowsMessages.WM_MOUSEMOVE && this.MouseDown) && this.Visible)
            {
                var x = lparam & 0xffff;
                var y = lparam >> 16;

                var bounds = this.GetBounds(this.Position);

                if (bounds.Contains(x, y))
                {
                    this.SetSliderValue(x);
                }

                this.MouseDown = true;
            }

            if (message == (ulong) WindowsMessages.WM_LBUTTONUP)
            {
                this.MouseDown = false;
            }
        }


        #endregion

        #region Methods

        /// <summary>
        ///     Sets the slider value.
        /// </summary>
        /// <param name="x">The x.</param>
        private void SetSliderValue(int x)
        {
            this.UpdateValue(Math.Max(this.MinValue, Math.Min(this.MaxValue, (int) ((x - this.Position.X) / (this.GetBounds(this.Position).Width - DefaultMenuTheme.LineWidth * 2) * this.MaxValue))));
        }

 
        private void UpdateValue(int newVal)
        {
            var oldClone = new MenuSlider { InternalName = this.InternalName, DisplayName = this.DisplayName, Value = this.Value, MinValue = this.MinValue, MaxValue = this.MaxValue };

            this.Value = newVal;

            this.Save();

            this.FireOnValueChanged(this, new ValueChangedArgs(oldClone, this));
        }



        /// <summary>
        ///    Loads the value from the file for this component
        /// </summary>
        internal override void LoadValue()
        {
            if (File.Exists(this.ConfigPath))
            {
                var read = File.ReadAllText(this.ConfigPath);

                var sValue = JsonConvert.DeserializeObject<MenuSlider>(read);

                if (sValue?.InternalName != null)
                {
                    this.Value = sValue.Value;
                }
            }
        }

        #endregion
    }
}