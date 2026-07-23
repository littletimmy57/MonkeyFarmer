using BTD_Mod_Helper;
using Gardener.Patches;
using MelonLoader;

[assembly: MelonInfo(typeof(Gardener.Main), Gardener.ModHelperData.Name, Gardener.ModHelperData.Version, Gardener.ModHelperData.Author)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace Gardener;

public class Main : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<Main>("The Monkey Farmer has joined the garden.");
    }

    public override void OnGameObjectsReset()
    {
        CropStandCashPatch.Reset();
    }
}
