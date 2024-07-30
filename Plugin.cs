using SML;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Server.Shared.Extensions;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using SalemModLoaderUI;
using Home.Common.Tooltips;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Home.LoginScene;
using System.Net.NetworkInformation;
using System;

namespace ModPageLib
{

	[Mod.SalemMod]
	class Main
	{
		public static Mod.ModInfo Mod;
		public static void Start()
		{
			Debug.Log("Working?");

		}
		public static void ChangeFontSize(int size)
		{
			PrepareVars.modDesc.fontSizeMax = size;
		}
	}
	[DynamicSettings]
	public class Settings
	{
		public ModSettings.IntegerInputSetting FontSize
		{
			get
			{
				ModSettings.IntegerInputSetting FontSize = new()
				{
					Name = "Font Size of the text",
					Description = "Changes the font size of the text in the mod page.",
					MaxValue = 100,
					MinValue = 0,
					DefaultValue = 30,
					AvailableInGame = true,
					Available = true,
					OnChanged = (s) => Main.ChangeFontSize(s)
				};
				return FontSize;
			}
		}
	}
	[HarmonyPatch(typeof(LoginSceneController), "Start")]
	class GetMod
	{
		public static void Prefix()
		{
			Main.Mod = ModStates.LoadedMods.First(x => x.HarmonyId == "JAN.modpagelib");
		}
	}
	[HarmonyPatch(typeof(SalemModLoaderMainMenuController), "CacheObjects")]
	class PrepareVars
	{
		public static GameObject modTemplate;
		static public GameObject modPage;
		static public GameObject modPageContent;
		static public GameObject modPageLoading;
		static public GameObject padding;
		public static TextMeshProUGUI modName;
		public static TextMeshProUGUI modDesc;
		public static Image logo;
		public static Transform viewport;
		public static VerticalLayoutGroup vlg;
		public static void Postfix(SalemModLoaderMainMenuController __instance)
		{
			//i haven't need to use reflection in so long lol, for some reason you can't publicize the SML dll
			modTemplate = (GameObject)__instance.GetType().GetField("manageModTabTemplate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			GameObject browse = (GameObject)__instance.GetType().GetField("browseMenu", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
			modPage = GameObject.Instantiate<GameObject>(browse, browse.transform.parent);
			//we love ui bs
			modPage.name = "ModPage Menu";
			modPage.transform.SetAsLastSibling();
			modPage.transform.Find("Mod Stuff/Searchbar").gameObject.SetActive(false);
			modPage.transform.Find("NoMods").gameObject.SetActive(false);
			modPage.transform.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => { modPage.SetInactiveIfNot(); });
			modPageLoading = modPage.transform.Find("Loading").gameObject;
			viewport = modPage.transform.Find("Mod Stuff/Mod List/Viewport");
			RectTransform browserTemplate = modPage.transform.Find("Mod Stuff/Mod List/Viewport/Content/BrowseTabTemplate") as RectTransform;
			modPageContent = browserTemplate.gameObject;
			foreach (Transform child in browserTemplate) //???? why
			{
				switch (child.name)
				{
					case "Thumbnail":
						logo = child.GetComponent<Image>();
						child.localScale *= 2;
						break;
					case "Mod Name":
						modName = child.GetComponent<TextMeshProUGUI>();
						modName.horizontalAlignment = HorizontalAlignmentOptions.Center;
						modName.fontSizeMin = 50;
						break;
					case "Mod Author":
						modDesc = child.GetComponent<TextMeshProUGUI>();
						modDesc.fontSizeMax = ModSettings.GetInt("Font Size of the text", "JAN.modpagelib");
						modDesc.horizontalAlignment = HorizontalAlignmentOptions.Left;
						modDesc.gameObject.AddComponent<TMPro_LinkOpener>();
						break;
					default:
						GameObject.Destroy(child.gameObject);
						break;
				}
			}
			modPageContent.AddComponent<LayoutElement>().minWidth = 800;
			RectTransform mpTransform = modPageContent.transform.parent as RectTransform;
			vlg = modPageContent.AddComponent<VerticalLayoutGroup>();
			vlg.childControlHeight = true;
			vlg.childControlWidth = true;
			vlg.childForceExpandHeight = true;
			vlg.childForceExpandWidth = true;
			vlg.childAlignment = TextAnchor.MiddleCenter;
			vlg.spacing = 71;
			VerticalLayoutGroup vlgContent = mpTransform.GetComponent<VerticalLayoutGroup>();
			vlgContent.childControlHeight = true;
			vlgContent.childControlWidth = true;
			vlgContent.childForceExpandHeight = true;
			vlgContent.childForceExpandWidth = true;
			vlgContent.childAlignment = TextAnchor.MiddleRight;
			mpTransform.anchorMax = new Vector2(1, 1);
			mpTransform.anchorMin = new Vector2(1, 0);
			GameObject thingie = new GameObject("Horizontal Layout");
			thingie.transform.SetParent(modPageContent.transform);
			thingie.transform.SetAsFirstSibling();
			padding = new GameObject("Padding");
			padding.transform.SetParent(modPageContent.transform);
			padding.transform.SetAsFirstSibling();
			Image img = padding.AddComponent<Image>();
			img.color = new Color(0.10196078431372549f, 0.10196078431372549f, 0.10196078431372549f);
			HorizontalLayoutGroup hlg = thingie.AddComponent<HorizontalLayoutGroup>();
			hlg.childControlHeight = false;
			hlg.childControlWidth = false;
			hlg.childForceExpandHeight = false;
			hlg.childForceExpandWidth = false;
			//hlg.childScaleHeight = true;
			hlg.childScaleWidth = true;
			hlg.childAlignment = TextAnchor.MiddleCenter;
			hlg.spacing = 0.175f;
			logo.transform.SetParent(thingie.transform);
			modName.transform.SetParent(thingie.transform);
			modPageContent.SetActive(false);
			modPage.SetInactiveIfNot();
		}
	}
	[HarmonyPatch(typeof(SalemModLoaderMainMenuController), "LoadManagedMod")]
	class AddModPageButton
	{
		static public void Postfix(SalemModLoaderMainMenuController __instance, Mod.ModInfo modInfo)
		{
			if ((modInfo.RequiredMods == null || !modInfo.RequiredMods.Contains("JAN.modpagelib")) && modInfo != Main.Mod) return;
			Transform loadedMod = PrepareVars.modTemplate.transform.parent.Find(modInfo.DisplayName);
			GameObject info = loadedMod.Find("Buttons/Info").gameObject;
			/*if (!ModSettings.GetBool("No new button.", "JAN.modpagelib"))
			{
				GameObject newButton = GameObject.Instantiate<GameObject>(info, info.transform.parent);
				newButton.transform.SetAsFirstSibling();
				newButton.name = "Mod Page";
				newButton.SetActiveIfNot();
				newButton.GetComponent<TooltipTrigger>().NonLocalizedString = "<color=#66FF66>(!) This mod has a mod page, click this button to open it.</color>";
				newButton.GetComponent<Button>().onClick.AddListener(OpenModPage(__instance, modInfo));
				newButton.GetComponent<Image>().color = Color.green;
			} 
			else
			{*/
			info.GetComponent<TooltipTrigger>().NonLocalizedString += "\n<color=#66FF66>(!) This mod has a mod page, click this button to open it.</color>";
			info.GetComponent<Button>().onClick.AddListener(OpenModPage(__instance, modInfo));
			info.GetComponent<Image>().color = Color.green;
			//}

		}
		public static UnityAction OpenModPage(SalemModLoaderMainMenuController instance, Mod.ModInfo modInfo)
		{
			return delegate ()
			{
				instance.StartCoroutine(OpenModPage(modInfo));
			};
		}
		static public Mod.ModInfo lastModInfo;
		static public IEnumerator OpenModPage(Mod.ModInfo modInfo)
		{

			PrepareVars.modPage.SetActive(true);
			//! if (lastModInfo == modInfo) { yield return null; yield return null; PrepareVars.viewport.localPosition = new Vector2(-497, PrepareVars.viewport.localPosition.y); yield break; }
			GameObject mpContent = PrepareVars.modPageContent;
			mpContent.SetInactiveIfNot();
			GameObject loading = PrepareVars.modPageLoading;
			loading.SetActiveIfNot();
			yield return new WaitForSeconds(0.2f);
			string modPageInfo = "";
			string asPath = modInfo.AssemblyPath;
			Assembly assembly = Assembly.LoadFile(asPath);
			string[] manifestResourceNames = assembly.GetManifestResourceNames();
			if (!manifestResourceNames.Any((string x) => x.ToLower().EndsWith("modpage.txt"))) { Debug.LogWarning("[ModPage Lib] This mod doesn't contain a ModPage! Are you sure you embedded modpage.txt?"); yield break; }
			using (Stream manifestResourceStream = assembly.GetManifestResourceStream(manifestResourceNames.First((string x) => x.ToLower().EndsWith("modpage.txt"))))
			{
				using (StreamReader streamReader = new StreamReader(manifestResourceStream))
				{
					modPageInfo = streamReader.ReadToEnd();
				}
			}
			PrepareVars.modDesc.SetText(modPageInfo + "\n<color=#FFFF99><size=100%><align=\"right\">- Made With <b>The ModPage Lib</b>.</align></size></color>");
			PrepareVars.modName.SetText($"{modInfo.DisplayName}{(ModSettings.GetBool("Titles with less info.", "JAN.modpagelib") ? "" : $"<size=50%>(v{modInfo.Version})\n By {string.Join(" & ", modInfo.Authors)}")}");

			if (modInfo.Thumbnail != null && !ModSettings.GetBool("Don't load images.", "JAN.modpagelib"))
			{
				PrepareVars.padding.SetActiveIfNot();
				PrepareVars.logo.gameObject.SetActiveIfNot();
				PrepareVars.logo.sprite = modInfo.Thumbnail;
			}
			else
			{
				PrepareVars.logo.gameObject.SetInactiveIfNot();
				PrepareVars.padding.SetInactiveIfNot();

			}
			Debug.Log($"{modInfo.DisplayName} has a mod page with info: {modPageInfo}");
			loading.gameObject.SetActive(false);
			mpContent.SetActive(true);
			PrepareVars.viewport.localPosition = new Vector2(-497, PrepareVars.viewport.localPosition.y);
			lastModInfo = modInfo;
			yield return null;
			if (PrepareVars.modName.textInfo.lineCount > 2 && !PrepareVars.logo.gameObject.activeSelf)
			{
				PrepareVars.modDesc.SetText("\n\n" + PrepareVars.modDesc.text);
			}
			PrepareVars.viewport.localPosition = new Vector2(-497, PrepareVars.viewport.localPosition.y);
			yield return null;
			PrepareVars.viewport.localPosition = new Vector2(-497, PrepareVars.viewport.localPosition.y);
		}
	}
}
