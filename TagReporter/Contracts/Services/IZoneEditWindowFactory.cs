﻿using TagReporter.Models;
using TagReporter.Views;

namespace TagReporter.Contracts.Services;

public interface IZoneEditWindowFactory
{
    public ZoneEditWindow Create(EditMode mode, Zone? zone);
}