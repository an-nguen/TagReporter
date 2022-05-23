using System;

namespace TagReporter.Models;

public readonly record struct Measurement(
    DateTimeOffset DateTime,
    double TemperatureValue,
    double Cap
);
