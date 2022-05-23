using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagReporter.Models;
using TagReporter.Views;

namespace TagReporter.Contracts.Services;

public interface IZoneGroupEditWindowFactory
{
    public ZoneGroupEditWindow Create(EditMode mode, ZoneGroup? group);
}