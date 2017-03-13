using System;
using System.Drawing;

using Comzept.Genesis.NetRix.VgxDraw;

namespace GuruComponents.Netrix.VmlDesigner.DataTypes
{
	/// <summary>
	/// Definition of a vector of Point.
	/// </summary>
	public class VgVector3D : IVgVector3D
	{
        private IVgVector3D nativeVector;
        private double z;

        private VgVector3D(double z)
        {
            this.z = z;
        }

		internal VgVector3D(IVgVector3D nativeVector)
		{
            this.nativeVector = nativeVector;
        }

        /// <summary>
        /// Gets or sets the vector in Pixel co-ordinates.
        /// </summary>
        public Point Pixel
        {
            get
            {
                return new Point(Convert.ToInt32(this.x / 0.75), Convert.ToInt32(this.y / 0.75));
            }
            set
            {
                this.x = value.X * 0.75;
                this.y = value.Y * 0.75;
            }
        }

        /// <summary>
        /// Used internally to set the attribute back. Later, when the native wrapper classes are ready, we 
        /// will remove this and let the wrapper classes handle the internal stuff.
        /// </summary>
        internal IVgVector3D NativeVector
        {
            get
            {
                return this.nativeVector;
            }
        }

        #region IVgVector3D Member

        public int Creator
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.Creator hinzuf�gen
                return 0;
            }
        }

        public double x
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.x hinzuf�gen
                return 0;
            }
            set
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.x hinzuf�gen
            }
        }

        public object Application
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.Application hinzuf�gen
                return null;
            }
        }

        public string value
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.value hinzuf�gen
                return null;
            }
            set
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.value hinzuf�gen
            }
        }

        public Comzept.Genesis.NetRix.VgxDraw.VgVectorType Type
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.Type hinzuf�gen
                return new Comzept.Genesis.NetRix.VgxDraw.VgVectorType ();
            }
        }

        public double y
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.y hinzuf�gen
                return 0;
            }
            set
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.y hinzuf�gen
            }                
        }

        public double Z
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.y hinzuf�gen
                return z;
            }
            set
            {
                z = value;
            }
        }

        public object Parent
        {
            get
            {
                // TODO:  Getter-Implementierung f�r VgVector3D.Parent hinzuf�gen
                return null;
            }
        }

        #endregion
    }
}
