﻿using System.Collections.Generic;

namespace Winterfern
{
    public class StackReport
    {
        public List<Solution> Results { get; set; } = new List<Solution>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}