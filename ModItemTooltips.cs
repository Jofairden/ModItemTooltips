using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ModItemTooltips
{
	// (c) Jofairden / Gorateron

	public class ModItemTooltips : Mod
	{
		public static ModItemTooltips instance;
		//public static bool cheatSheetEnabled;

		public ModItemTooltips()
		{
			Properties = new ModProperties()
			{
				Autoload = true
			};
		}

		//public override void ChatInput(string text, ref bool broadcast)
		//{
		//	if (text.Length > 1 && text[0] == '/')
		//	{
		//		string command = text.Substring(1);
		//		if (command.ToUpper() == "MODITEMTOOLTIPS")
		//		{
		//			var mPlayer = Main.LocalPlayer.GetModPlayer<ModItemTooltipsPlayer>(this);
		//			mPlayer.enabledTitleTT = !mPlayer.enabledTitleTT;
		//			string toggleText = mPlayer.enabledTitleTT ? "on" : "off";
		//			Main.NewText($"Toggled ModItemTooltips {toggleText}", 255, 255, 0);
		//		}
		//	}
		//}

		public override void Load()
		{
			instance = this;
			//cheatSheetEnabled = ModLoader.GetMod("CheatSheet") != null;
		}
	}

	public class ModItemTooltipsPlayer : ModPlayer
	{
		public bool enabledTitleTT = true;

		public override void OnEnterWorld(Player player)
		{
			string toggleText = player.GetModPlayer<ModItemTooltipsPlayer>(mod).enabledTitleTT ? "on" : "off";
			Main.NewText($"ModItemTooltips is {toggleText} for {player.name}", 255, 255, 0);
		}

		public override TagCompound Save()
		{
			return new TagCompound()
			{
				["enabledTitleTT"] = enabledTitleTT
			};
		}

		public override void Load(TagCompound tag)
		{
			enabledTitleTT = tag.GetBool("enabledTitleTT");
		}
	}

	public class ModItemTooltipInfo : ItemInfo
	{
		public static ModItemTooltipInfo GetInfo(Item item, Mod mod) =>
			item.GetModInfo<ModItemTooltipInfo>(mod);

		public string crafter = null;

		public override ItemInfo Clone()
		{
			var clone = new ModItemTooltipInfo {crafter = crafter};
			return clone;
		}
	}

	public class ModItemTooltipsGlobalItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!Main.LocalPlayer.GetModPlayer<ModItemTooltipsPlayer>(mod).enabledTitleTT || item.modItem == null) return;

			var layer =
				tooltips
					.FirstOrDefault(x =>
						x.mod == "Terraria"
						&& x.Name == "ItemName");

			string addedText = $" [{item.modItem.mod.DisplayName}]";

			if (layer != null
				&& !layer.text.EndsWith(addedText.Trim()))
				layer.text += addedText;

			var info = ModItemTooltipInfo.GetInfo(item, mod);

			if (info.crafter != null)
			{
				tooltips.Insert(tooltips.IndexOf(layer), new TooltipLine(mod, "CrafterTooltip", $"Crafted by: {info.crafter}"));
			}
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			var info = ModItemTooltipInfo.GetInfo(item, mod);
			writer.Write(info.crafter);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			var info = ModItemTooltipInfo.GetInfo(item, mod);
			info.crafter = reader.ReadString();
		}
	}
}
