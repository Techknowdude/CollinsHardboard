using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Configuration_windows.Annotations;
using ImportLib;
using ModelLib;

namespace Configuration_windows
{
    /// <summary>
    /// Contains configuration data
    /// </summary>
    [Serializable]
    public class ItemIngredient : INotifyPropertyChanged
    {
        #region Fields
        private Int32 _itemsIn;
        private Int32 _itemInId = -1;
        private Int32 _itemsOut;

        #endregion

        #region Properties

        /// <summary>
        /// The number of pieces that is used in the conversion
        /// </summary>
        public Int32 ItemsIn
        {
            get { return _itemsIn; }
            set
            {
                if (value != _itemsIn)
                {
                    _itemsIn = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// The number of pieces that is used in the conversion
        /// </summary>
        public Int32 ItemsOut
        {
            get { return _itemsOut; }
            set
            {
                if (value != _itemsOut)
                {
                    _itemsOut = value;
                    OnPropertyChanged();
                }
            }

        }

        /// <summary>
        /// The product master ID for the input items
        /// </summary>
        public Int32 ItemInID
        {
            get { return _itemInId; }
            set
            {
                if (value != _itemInId)
                {
                    _itemInId = value;
                    ItemIn = StaticInventoryTracker.ProductMasterList.FirstOrDefault(x => x.MasterID == value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Reference to the master item for the input
        /// </summary>
        public ProductMasterItem ItemIn { get; set; }


        #endregion

        /// <summary>
        /// Factory for Configuration
        /// </summary>
        /// <returns>A new Configuration</returns>
        public static ItemIngredient Create(Int32 inputID = default(int),Int32 inputPieces = 1,Int32 outputPieces = default(int), double convertionsPerMin = default(double))
        {
            //TODO: Remove conversion time
            return new ItemIngredient(inputID, inputPieces, outputPieces);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        ItemIngredient(Int32 inputID = default(int),Int32 inputPieces = default(int),Int32 outputPieces = default(int))
        {
            ItemsIn = inputPieces;
            ItemInID = inputID;
            ItemsOut = outputPieces;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Saves the configuration using the binary writer
        /// </summary>
        public void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream, this);
        }

        /// <summary>
        /// Create a new configuration from a file
        /// </summary>
        /// <param name="reader">input stream containing the configuration data.</param>
        /// <returns>New Configuration with the data in the input stream</returns>
        public static ItemIngredient Create(Stream stream, IFormatter formatter)
        {
            return (ItemIngredient) formatter.Deserialize(stream);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ItemIngredient;
            if (other == null)
            {
                return base.Equals(obj);
            }
            else
            {
                return  other.ItemIn == ItemIn 
                       && other.ItemInID == ItemInID
                       && other.ItemsIn == ItemsIn 
                       && other.ItemsOut == ItemsOut 
                       ;
            }
        }

    }
}
