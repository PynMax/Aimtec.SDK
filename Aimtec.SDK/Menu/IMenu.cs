﻿namespace Aimtec.SDK.Menu
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface IMenu
    /// </summary>
    /// <seealso cref="Aimtec.SDK.Menu.IMenuComponent" />
    public interface IMenu
    {
        #region Public Properties

        /// <summary>
        ///     Gets the children.
        /// </summary>
        /// <value>The children.</value>
        Dictionary<string, MenuComponent> Children { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Adds the specified identifier.
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <returns>IMenu.</returns>
        Menu Add(MenuComponent menu);

        /// <summary>
        ///     Attaches this instance.
        /// </summary>
        /// <returns>IMenu.</returns>
        Menu Attach();

        #endregion
    }
}