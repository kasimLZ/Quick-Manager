﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Database.Base.Interface.Infrastructure
{
    public interface IDatabaseFactory
    {
        IApplicationDb DataContext { get; }
    }
}
