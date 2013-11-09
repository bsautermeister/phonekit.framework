﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Store;
using Store = Windows.ApplicationModel.Store;
using PhoneKit.Framework.InAppPurchase;

namespace PhoneKit.Framework.Controls
{
    /// <summary>
    /// The in-app store control
    /// </summary>
    public partial class InAppStoreControl : UserControl
    {
        #region Members

        /// <summary>
        /// The localized loading text as a dependency property.
        /// </summary>
        public static readonly DependencyProperty InAppStoreLoadingTextProperty =
            DependencyProperty.Register("InAppLoadingProductsText",
            typeof(string), typeof(InAppStoreControl),
            new PropertyMetadata("Loading..."));

        /// <summary>
        /// The localized no products text as a dependency property.
        /// </summary>
        public static readonly DependencyProperty InAppStoreNoProductsTextProperty =
            DependencyProperty.Register("InAppStoreNoProductsText",
            typeof(string), typeof(InAppStoreControl),
            new PropertyMetadata("No in-app product available."));

        /// <summary>
        /// The localized purchased text as a dependency property.
        /// </summary>
        public static readonly DependencyProperty InAppStorePurchasedTextProperty =
            DependencyProperty.Register("InAppStorePurchasedText",
            typeof(string), typeof(InAppStoreControl),
            new PropertyMetadata("Purchased"));

        /// <summary>
        /// The list of loaded products.
        /// </summary>
        private IList<ProductItem> _loadedProducts = new List<ProductItem>();

        /// <summary>
        /// The supported product IDs as a comma seperated string list
        /// </summary>
        private string _supportedProductIds = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an InAppStoreControl instance.
        /// </summary>
        public InAppStoreControl()
        {
            InitializeComponent();
            
            ShowMessage(InAppStoreLoadingText);

            // shoe loading message und update products when page has loaded
            Loaded += (s, e) => {
                UpdateProducts();
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the products of the in-app store.
        /// </summary>
        private async void UpdateProducts()
        {
            // verify supported product configuration
            if (string.IsNullOrEmpty(_supportedProductIds))
                throw new InvalidOperationException("There are no supported products.");

            _loadedProducts.Clear();

            // load products
            _loadedProducts = await InAppPurchaseHelper.LoadProductsAsync(_supportedProductIds.Split(',').ToList(),
                InAppStorePurchasedText);

            if (_loadedProducts.Count > 0)
            {
                HideMessage();

                // display loaded products
                ProductItemsList.ItemsSource = _loadedProducts;

                ProductsInTransition.Begin();
            }
            else
            {
                ShowMessage(InAppStoreNoProductsText);
            }
        }

        /// <summary>
        /// Displays an info message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowMessage(string message)
        {
            MessageInfo.Visibility = Visibility.Visible;
            MessageInfo.Text = message;
        }

        /// <summary>
        /// Hides the info message.
        /// </summary>
        private void HideMessage()
        {
            MessageInfo.Visibility = Visibility.Collapsed;
            MessageInfo.Text = string.Empty;
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when a new products was selected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private async void ProductItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (ProductItemsList.SelectedIndex < 0)
                return;

            // get the associated product ID
            string productId = _loadedProducts[ProductItemsList.SelectedIndex].Id;

            // purchase product if possible
            if (!InAppPurchaseHelper.IsProductActive(productId))
            {
                await InAppPurchaseHelper.RequestProductPurchaseAsync(productId);

                UpdateProducts();
            }

            // Reset selected index to -1 (no selection)
            ProductItemsList.SelectedIndex = -1;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the localized loading text.
        /// </summary>
        public string InAppStoreLoadingText
        {
            get { return (string)GetValue(InAppStoreLoadingTextProperty); }
            set { SetValue(InAppStoreLoadingTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the localized purchased text.
        /// </summary>
        public string InAppStorePurchasedText
        {
            get { return (string)GetValue(InAppStorePurchasedTextProperty); }
            set { SetValue(InAppStorePurchasedTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the localized no products text.
        /// </summary>
        public string InAppStoreNoProductsText
        {
            get { return (string)GetValue(InAppStoreNoProductsTextProperty); }
            set { SetValue(InAppStoreNoProductsTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the supported product IDs as a comma seperated string.
        /// </summary>
        public string SupportedProductIds
        {
            get
            {
                return _supportedProductIds;
            }
            set
            {
                _supportedProductIds = value;
            }
        }

        #endregion
    }
}
