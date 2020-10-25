using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class Localization : MonoBehaviour
{
	[SerializeField] private Canvas[] canvas;
	[SerializeField] private Dropdown dropdown;
	[SerializeField] private LocalizationComponent[] source;
	public int index;
	private TextAsset[] binary;
	private static Localization _internal;
	private static int globalindex;

	public static Localization Internal
	{
		get { return _internal; }
	}

	void Awake()
	{
		index = PlayerPrefs.GetInt("index");
		if(index != globalindex)
		{
			globalindex = index;
		}
		
		_internal = this;
		StartScene();
		dropdown.value = index;
	}

	public void Custom(int id, int index)
	{
		foreach (LocalizationComponent t in source)
		{
			if (t.hash == id) t.SetCustom(index);
		}
	}

	void StartScene()
	{
		Load();
		dropdown.value = index;
		if (index == 0)
		{
			dropdown.value = -1;
		}
	}
	void Update()
	{   
		if (index != dropdown.value)
		{
			index = dropdown.value;
			PlayerPrefs.SetInt("index", index);
			PlayerPrefs.Save();
			globalindex = index;
		}

	}
	void Load()
	{
		binary = Resources.LoadAll<TextAsset>("Localization");
		dropdown.options = new List<Dropdown.OptionData>();

		if (binary.Length == 0)
		{
			ListData("List empty...");
			dropdown.value = -1;
			Debug.Log(this + " файлы не обнаружены.");
			return;
		}

		for (int i = 0; i < binary.Length; i++)
		{
			ListData(binary[i].name);
		}

		dropdown.onValueChanged.AddListener(delegate { Locale(); });
	}

	void ListData(string value)
	{
		Dropdown.OptionData option = new Dropdown.OptionData();
		option.text = value;
		dropdown.options.Add(option);
	}

	int GetInt(string text)
	{
		int value;
		if (int.TryParse(text, out value)) return value;
		return 0;
	}

	void InnerText(int id, string text)
	{
		foreach (LocalizationComponent t in source)
		{
			if (t.hash == id) t.target.text = text;
		}
	}

	void InnerCustomText(int id, string text)
	{
		foreach (LocalizationComponent t in source)
		{
			if (t.hash == id) t.SetCustomLoad(text);
		}
	}

	void Locale()
	{
		XmlTextReader reader = new XmlTextReader(new StringReader(binary[dropdown.value].text));
		while (reader.Read())
		{
			if (reader.IsStartElement("content")) InnerText(GetInt(reader.GetAttribute("id")), reader.ReadString());
			else if (reader.IsStartElement("custom")) InnerCustomText(GetInt(reader.GetAttribute("id")), reader.ReadString());
		}
		reader.Close();
	}

	public void SetComponents()
	{
		source = LocalizationGenerator.GenerateLocale(canvas);
	}
}
