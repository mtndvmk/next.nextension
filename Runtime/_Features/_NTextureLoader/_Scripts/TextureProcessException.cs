using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension.TextureLoader
{
    public class TextureProcessException : Exception
    {
       public TextureProcessException(string message) : base(message) { }
    }
}
