using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMod.Exceptions
{
	public class SharpModException: Exception
	{
        public string ErrorFile { get; set; }

        public SharpModException(String message):base(message)
        {
           
        }

        public SharpModException(String message,String errorFile)
            : this(message)
        {
            this.ErrorFile = errorFile;
        }

        public SharpModException(String message,Exception baseException)
            : base(message, baseException)
        {
        }
	}
}
