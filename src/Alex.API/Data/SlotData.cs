﻿using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using Alex.API.Items;
using fNbt;

namespace Alex.API.Data
{
    public class SlotData
    {
	    public int ItemID = -1;
	    public short ItemDamage = 0;
	    public byte Count = 0;

		public NbtCompound Nbt = null;

		public SlotData()
	    {
		    
	    }
    }
}
