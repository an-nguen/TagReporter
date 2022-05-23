namespace TagReporter.DTOs;

public class TagResponse
{
    public string? __type { get; set; }
    public string? managerName { get; set; }
    public string? mac { get; set; }
    public object[]? mirrors { get; set; }
    public int dbid { get; set; }
    public string? notificationJS { get; set; }
    public string? name { get; set; }
    public string? uuid { get; set; }
    public string? comment { get; set; }
    public int slaveId { get; set; }
    public int tagType { get; set; }
    public object? discon { get; set; }
    public long lastComm { get; set; }
    public bool alive { get; set; }
    public int signaldBm { get; set; }
    public double batteryVolt { get; set; }
    public bool beeping { get; set; }
    public bool lit { get; set; }
    public bool migrationPending { get; set; }
    public int beepDurationDefault { get; set; }
    public int eventState { get; set; }
    public int tempEventState { get; set; }
    public bool OutOfRange { get; set; }
    public int tempSpurTh { get; set; }
    public int lux { get; set; }
    public double temperature { get; set; }
    public double tempCalOffset { get; set; }
    public double capCalOffset { get; set; }
    public object? image_md5 { get; set; }
    public double cap { get; set; }
    public int capRaw { get; set; }
    public int az2 { get; set; }
    public int capEventState { get; set; }
    public int lightEventState { get; set; }
    public bool shorted { get; set; }
    public object? zmod { get; set; }
    public object? thermostat { get; set; }
    public object? playback { get; set; }
    public int postBackInterval { get; set; }
    public uint rev { get; set; }
    public uint version1 { get; set; }
    public int freqOffset { get; set; }
    public int freqCalApplied { get; set; }
    public int reviveEvery { get; set; }
    public int oorGrace { get; set; }
    public object? tempBL { get; set; }
    public object? capBL { get; set; }
    public object? luxBL { get; set; }
    public double LBTh { get; set; }
    public bool enLBN { get; set; }
    public int txpwr { get; set; }
    public bool rssiMode { get; set; }
    public bool ds18 { get; set; }
    public int v2flag { get; set; }
    public double batteryRemaining { get; set; }

}