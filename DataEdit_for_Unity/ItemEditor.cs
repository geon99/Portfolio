using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SpaceShip;

public class ItemEditor : EditorWindow
{
	public static ItemEditor window;

	[MenuItem ("Window/Item Editor", false, 10)]
	public static void Init () 
	{
		window = (ItemEditor)EditorWindow.GetWindow (typeof(ItemEditor));
		window.InitTable ();
	}
	public static ItemEditor GetEditor ()
	{
		if (window == null)
			Init ();

		return window;
	}

	//  text, edit style
	public  TextAsset       itemAsset;

	//  contant
	public  TableConstDic   tableConst = TableInit.GetTableConst ();
	public  DicAbilityRate  editAbillR = TableInit.GetAbllityRate ();
	public  string          editConst = "";
	public  int[]           modelCode;
	public  string[]        modelName;
	public  float[]         popupRat  = new float [] {0, 0, 0, 1f/8f, 1f/4f, 1f/3f, 1f/2f, 2f/3f, 3f/4f, 1f};
	public  string[]        popupCmd;

	//  edit data
	public  DictItem		tItemDict = new DictItem();
	public  List<TableItem> nowList = new List<TableItem>();
	public  int             nowModel = 101;
	public	string			mIDFilter = "";
	public  bool			showAll = true;
	public  int             modelLastID;
	public  TableItem       newItem;

	public  int             changeID;
	public  int             changeEditID;
	public  bool            showConst;
	public  bool            refreshgain;
	public  bool            isnewlist;
	private Vector2         scrollpos;

	#region Init Table/UI
	void		InitTable ()
	{
		// popup text
		float[] srcpop = popupRat;
		popupCmd = new string[srcpop.Length];
		popupCmd[1] = "Remain";
		for (int i = 2; i < srcpop.Length; i++)
			popupCmd[i] = string.Format ("{0} %", (int)(srcpop[i] * 100));

		// item list
		if (itemAsset == null)
		{
			GameObject scene = GameObject.FindWithTag ("Scene");
			if (scene != null)
			{
				MainData.main = scene.GetComponentInChildren<MainData>();
				if (MainData.main != null)
					itemAsset = MainData.main.itemAsset;
			}
		}
		if (itemAsset != null)
		{
			LoadItems (GetItemPathFile ());
		}

		// model list
		if (tableConst.modelW != null)
		{
			List<int> codeList = new List<int> ();
			foreach (var pair in tableConst.modelW)
				codeList.Insert (0, (int)pair.Key);
			codeList.Sort ();
			
			modelName = new string[tableConst.modelW.Count];
			modelCode = new int   [tableConst.modelW.Count];
			for (int i = 0; i < codeList.Count; i++)
			{
				modelName[i] = codeList[i].ToString();
				modelCode[i] = codeList[i];
			}
		}
	}
	#endregion
	#region File IO
	public static string GetItemPath ()
	{
		return "Assets/[Shotting]/Data/";
	}
	string		GetItemPathFile ()
	{
		if (itemAsset == null)
			return "";

		string path = AssetDatabase.GetAssetPath(itemAsset);

		return path;
	}

	void 		SaveItems (string itemFile)
	{
		tItemDict = SortItemDict (tItemDict);

		if (itemFile != null && itemFile != "")
		{
			using (StreamWriter sw = new StreamWriter (itemFile))
			{
				// save const
				TableReader.ItemConst_Save (sw.WriteLine, tableConst);

				// save item
				TableReader.DictItem_Save (sw.WriteLine, tItemDict);
			}

			using (SaveEncode encode = new SaveEncode (itemFile))
			{
				// save const
				TableReader.ItemConst_Save (encode.WriteLine, tableConst);
				
				// save item
                TableReader.DictItem_Save (encode.WriteLine, tItemDict);
            }
		}

		AssetDatabase.Refresh ();

		int k = itemFile.IndexOf ("Assets/");
		if (k > 0)
			itemFile = itemFile.Substring (k, itemFile.Length - k);

		itemAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(itemFile, typeof(TextAsset));
	}
	void 		LoadItems (string itemFile)
	{
		if (itemFile != null && itemFile != "")
		{
			using (StreamReader sr = new StreamReader (itemFile))
			{
				// read const
				tableConst = TableReader.ItemConst_Read (sr.ReadLine);
			}
			editAbillR = new DicAbilityRate (tableConst.abillR);
            
			using (StreamReader sr = new StreamReader (itemFile))
            {
				// read item
				tItemDict = TableReader.DictItem_Read (sr.ReadLine);
			}
		}
		
		int k = itemFile.IndexOf ("Assets/");
		if (k > 0)
			itemFile = itemFile.Substring (k, itemFile.Length - k);
		
		itemAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(itemFile, typeof(TextAsset));
	}

	void		DefaultLoad ()
	{
		if (tItemDict == null || tItemDict.Count == 0)
		{
			if (itemAsset != null)
			{
				LoadItems (GetItemPathFile ());
				
				ReadyNowList ();
			}
		}
	}
	void		MenuFileIO ()
	{
		if (GUILayout.Button ("Save..", GUILayout.Width(50)))
		{
			string newfile = EditorUtility.SaveFilePanel ("Save File", GetItemPath (), "", "txt");
			if (newfile.Length != 0)
			{
				SaveItems (newfile);
			}
		}
		if (GUILayout.Button ("Load..", GUILayout.Width(50)))
		{
			string newfile = EditorUtility.OpenFilePanel ("Load File", GetItemPath (), "txt");
			if (newfile.Length != 0)
			{
				LoadItems (newfile);
			}
			
			ReadyNowList ();
		}
		EditorGUILayout.LabelField ("File", GUILayout.Width(40));
		TextAsset newiAsset = (TextAsset)EditorGUILayout.ObjectField (itemAsset, typeof(TextAsset), true);
		if (newiAsset != itemAsset)
		{
			itemAsset = newiAsset;
			LoadItems (GetItemPathFile ());

			ReadyNowList ();
		}
		if (tItemDict.Count > 0)
		{
			if (GUILayout.Button ("Save", GUILayout.Width(50)))
			{
				SaveItems (GetItemPathFile ());
			}
		}
		if (GUILayout.Button ("Load", GUILayout.Width(100)))
		{
			LoadItems (GetItemPathFile ());

			ReadyNowList ();
		}
	}
	#endregion
	#region internal
	void 		ReadyNowList ()
	{
		nowList = new List<TableItem>();

		foreach (var pair in tItemDict)
		{
			if (CheckFilter (pair.Value.itemID))
			{
				nowList.Add (pair.Value);
				if (modelLastID < pair.Key)
					modelLastID = pair.Key;
			}
		}

		nowList.Sort(Compare);

		newItem.itemID = FindEmptyID (newItem.itemID);

		isnewlist = true;
	}
	int     	Compare(TableItem x, TableItem y)
	{
		return (int)x.itemID - (int)y.itemID;
	}

	bool		CheckFilter (int id)
	{
		if (showAll)
			return true;

		string model = id.ToString ();

		if (mIDFilter.Contains ("*"))
		{
			for (int k = 0; k < model.Length && k < mIDFilter.Length; k++)
				if (model[k] != mIDFilter[k] && mIDFilter[k] != '*')
					return false;

			return true;
		}
		else
		{
			if (model.Contains (mIDFilter))
				return true;
		}

		return false;
	}
	IntID 		MoveModel (IntID nowModel, int move)  // 모델의 이전 또는 이후 코드를 얻는다.
	{
		for (int i = 0; i < modelCode.Length; i++)
		{
			if (modelCode[i] == nowModel)
				return modelCode[(i+move + modelCode.Length) % modelCode.Length];
		}
		
		return nowModel;
	}
	IntID		FindEmptyID (IntID id)
	{
		foreach (TableItem item in nowList)
		{
			if (item.itemID == id)
				id = id + 1;
		}

		return id;
	}
	DictItem	SortItemDict (DictItem itemdic)
	{
		List<TableItem> list = new List<TableItem>();
		foreach (var pair in tItemDict)
			list.Add (pair.Value);

		list.Sort(Compare);

		DictItem dic = new DictItem();
		foreach (TableItem item in list)
			dic.Add (item.itemID, item);

		return dic;
	}

	int  		CaleCost100 (TableItem item, TableConstDic tc)
	{
		int totalw = 0;
		totalw += item.genShield  * tc.abillR[ItemAbility.genShield];
		totalw += item.genEnergy  * tc.abillR[ItemAbility.genEnergy];
		totalw += item.capShield  * tc.abillR[ItemAbility.capShield];
		totalw += item.capEnergy  * tc.abillR[ItemAbility.capEnergy];
		totalw += item.capStorage * tc.abillR[ItemAbility.capStorage];
		totalw += item.capAccel   * tc.abillR[ItemAbility.capAccel];
		totalw += item.capSpeed   * tc.abillR[ItemAbility.capSpeed];
		totalw += tc.skillW[(ItemSkill)item.skill] * 100;

		return totalw;
	}
	int  		CaleGain (TableItem item)
	{
		int totalw = CaleCost100 (item, tableConst);

		return (totalw - item.capBody * tableConst.abillR[ItemAbility.capBody]) / 100;
	}
	int  		calcAutoValue (int value, int preRate, int newRate, TableItem item)
	{
		int totalw = CaleCost100 (item, tableConst) - item.gain * 100;

		int cost = item.capBody * tableConst.abillR[ItemAbility.capBody] - (totalw - value * preRate);
		
		return cost / newRate;
	}

	void        AttachItem (int itemID)
	{
		Body body = UToolS.SelectBody ();
		if (body != null)
		{
			var partlist = Creation.ReadShipObject (body.gameObject); OpShip.OSIDE seldir;
			var selpart = OpShip.GetInsertParent (partlist, itemID, out seldir);
			TablePart newpart = OpShip.NewInsertItem (selpart, seldir, itemID);

			Item item = Creation.MakeItemObject (newpart, body.transform);
			item.renderer.material = MainRenderer.GetMeterial (item.gameObject, body.tone);

			if (newpart.posx != 0)
			{
				TablePart newpair = newpart;
				newpair.posx = -newpart.posx;
				newpair.flip = (byte)(newpart.flip == 1 ? 0 : 1);
				if (newpart.angle == 90 || newpart.angle == -90 || newpart.angle == 270)
					newpair.angle = IMath.GetCenterDegree(newpart.angle + 180);

				Item itemPair = Creation.MakeItemObject (newpair, body.transform);
				itemPair.renderer.material = MainRenderer.GetMeterial (itemPair.gameObject, body.tone);
			}

			Creation.DepthSort (body.transform);
		}
	}
	#endregion

	#region Edit 
	// model weight
	void 		EditConstWeightAllItem ()
	{
		DictItem newDic = new DictItem ();
		foreach (var pair in tItemDict)
		{
			TableItem item = pair.Value;
			int model = item.itemID / 100;
			if (tableConst.modelW.ContainsKey(model))
				item.capBody = tableConst.modelW[model];
			newDic[pair.Key] = item;
		}

		tItemDict = newDic;

		ReadyNowList ();
	}

	// ability rate
	TableItem	EditConstAuto (TableItem item)
	{
		switch (editConst)
		{
		case "capEnergy":
			item.capEnergy = calcAutoValue (item.capEnergy, tableConst.abillR[ItemAbility.capEnergy], editAbillR[ItemAbility.capEnergy], item);
			break;

		case "genEnergy":
			item.genEnergy = calcAutoValue (item.genEnergy, tableConst.abillR[ItemAbility.genEnergy], editAbillR[ItemAbility.genEnergy], item);
			break;

		case "capBody":
			item.capBody   = calcAutoValue (item.capBody  , tableConst.abillR[ItemAbility.capBody]  , editAbillR[ItemAbility.capBody]  , item);
			break;

		case "capShield":
			item.capShield = calcAutoValue (item.capShield, tableConst.abillR[ItemAbility.capShield], editAbillR[ItemAbility.capShield], item);
			break;

		case "genShield":
			item.genShield = calcAutoValue (item.genShield, tableConst.abillR[ItemAbility.genShield], editAbillR[ItemAbility.genShield], item);
			break;
			
		case "capAccel":
			item.capAccel = calcAutoValue (item.capAccel, tableConst.abillR[ItemAbility.capAccel], editAbillR[ItemAbility.capAccel], item);
			break;

		case "capSpeed":
			item.capSpeed = calcAutoValue (item.capSpeed, tableConst.abillR[ItemAbility.capSpeed], editAbillR[ItemAbility.capSpeed], item);
			break;
		}

		return item;
	}
	void 		EditConstRateAllItem ()
	{
		DictItem newDic = new DictItem();
		foreach (var pair in tItemDict)
		{
			TableItem newitem = EditConstAuto (pair.Value);
			newDic[pair.Key] = newitem;
		}

		tItemDict = newDic;

		tableConst.abillR = new DicAbilityRate (editAbillR);

		ReadyNowList ();
	}

	// ability value
	int  		EditorConstValue (int value, int width, string edit, int source)
	{
		int newvalue = EditorGUILayout.IntField (value, (value == source ? UTool.stTxtLit : UTool.stTxtRed), GUILayout.Width(width));
		if (editConst == "" && newvalue != value)
			editConst = edit;

		return (editConst == edit) ? newvalue : value;
	}
	int  		EditorValuePop (int value, TableItem item, int rate, int width, ref bool change)
	{
		int newvalue = EditorGUILayout.IntField (value, value == 0 ? GUI.skin.textField : UTool.stTxtAct, GUILayout.Width(width-14));
		if (newvalue != value)
			change = true;

		int newCmd = EditorGUILayout.Popup (0, popupCmd, GUILayout.Width(10));
		if (newCmd != 0)
		{
			if (newCmd == 1)
				newvalue = -CaleGain (item) * 100 / rate;
			else
				newvalue = (int)(item.capBody * 100 / rate * popupRat[newCmd]);

			change = true;
		}

		return newvalue;
	}
	TableItem	GUIEditor_Item (TableItem item, ref bool change, bool edititem = false) // ablility list
	{
		int newIntValue;

		if (edititem == false)
		{
			if (GUILayout.Button (item.itemID.ToString(), GUILayout.Width(60)))
			{
				changeID     = item.itemID;
				changeEditID = item.itemID;
			}
		}
		else
		{
			newIntValue = EditorGUILayout.IntField ((int)item.itemID, GUILayout.Width(60));
			if (newIntValue != (int)item.itemID)
			{
				item.itemID = FindEmptyID (newIntValue);
				change = true;
			}
		}

		item.model      = (short)(item.itemID / 100);

		newIntValue     = EditorGUILayout.IntField (item.scale, UTool.stTxtLit, GUILayout.Width(30));
		if (newIntValue != item.scale)
		{
			item.scale = newIntValue;
			change = true;
		}

		ItemSkill newsk = (ItemSkill)EditorGUILayout.EnumPopup (item.skill, item.skill == 0 ? GUI.skin.textField : UTool.stTxtAct, GUILayout.Width(60));
		if (newsk != item.skill)
		{
			item.skill = newsk;
			change = true;
		}

		item.capEnergy  = EditorValuePop (item.capEnergy , item, tableConst.abillR[ItemAbility.capEnergy], 60, ref change);
		item.genEnergy  = EditorValuePop (item.genEnergy , item, tableConst.abillR[ItemAbility.genEnergy], 60, ref change);

		//EditorGUILayout.LabelField (item.capBody.ToString(), GUILayout.Width(55));
		int newbody = EditorGUILayout.IntField (item.capBody, UTool.stTxtLit, GUILayout.Width(55-14));
		if (item.capBody != newbody)
		{
			item.capBody = newbody;
			change = true;
		}
		int newCmd = EditorGUILayout.Popup(2, new string[] {"Model", ""}, GUILayout.Width(10));
		if (newCmd == 0)
		{
			if (tableConst.modelW.ContainsKey(item.model))
			{
				item.capBody = tableConst.modelW[item.model] * 100 / tableConst.abillR[ItemAbility.capBody];
				change = true;
			}
		}

		item.capShield  = EditorValuePop (item.capShield , item, tableConst.abillR[ItemAbility.capShield ], 60, ref change);
		item.genShield  = EditorValuePop (item.genShield , item, tableConst.abillR[ItemAbility.genShield ], 60, ref change);
		item.capStorage = EditorValuePop (item.capStorage, item, tableConst.abillR[ItemAbility.capStorage], 60, ref change);
		item.capAccel   = EditorValuePop (item.capAccel  , item, tableConst.abillR[ItemAbility.capAccel  ], 60, ref change);
		item.capSpeed   = EditorValuePop (item.capSpeed  , item, tableConst.abillR[ItemAbility.capSpeed  ], 60, ref change);

		newIntValue     = EditorGUILayout.IntField (item.capSight  , GUILayout.Width(60));
		if (newIntValue != item.capSight)
		{
			item.capSight = newIntValue;
			change = true;
		}

		if (change || edititem || refreshgain || isnewlist)
		{
			isnewlist = false;

			newIntValue = CaleGain (item);
			if (item.gain != newIntValue)
			{
				item.gain = newIntValue;
				change = true;
			}
		}
		EditorGUILayout.LabelField (item.gain.ToString(), item.gain == 0 ? GUI.skin.label : UTool.stLabSpc, GUILayout.Width(30));

		if (GUILayout.Button ("Add", GUILayout.Width(40)))
		{
			AttachItem (item.itemID);
		}

		return item;
	}
	#endregion

	#region System
	void OnGUI () 
	{
		UTool.CheckInitUI ();

		bool change = false;

		#region File Menu
		EditorGUILayout.BeginHorizontal ();
		showConst = EditorGUILayout.Foldout (showConst, "Show Const");
		GUILayout.Space (20);
		DefaultLoad ();
		MenuFileIO ();

		EditorGUILayout.Space();
		if (GUILayout.Button ("Init", GUILayout.Width(50)) || tableConst.modelW == null)
		{
			InitTable ();
		}
		EditorGUILayout.EndHorizontal ();
		#endregion

		#region Edit Const Data
		if (showConst)
		{
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (20);
			for (int i = 0; i < modelCode.Length; i++)
			{
				IntID model = modelCode[i];
				if (tableConst.modelW.ContainsKey (model) == false)
					continue;

				EditorGUILayout.LabelField (model.ToString(), GUILayout.Width(25));
				int w = EditorConstValue (tableConst.modelW[model], 40, "model_weight", tableConst.modelW[model]);
				if (w != tableConst.modelW[model])
				{
					tableConst.modelW[model] = w;
					break;
				}

				if (i+1 < modelCode.Length) // 종류가 다르면 다음라인에 출력.
				{
					int r1 = modelCode[i  ] / 100;
					int r2 = modelCode[i+1] / 100;
					if (r1 != r2)
					{
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						GUILayout.Space (20);
					}
				}
			}
			if (GUILayout.Button ("Apply", GUILayout.Width(50)))
			{
				EditConstWeightAllItem ();
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal (); // [skill weight]
			GUILayout.Space (20);
			int count = 0;
			foreach (var pair in tableConst.skillW)
			{
				if (pair.Key == ItemSkill.none)
					continue;

				if (count == 7)
				{
					count = 0;
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginHorizontal ();
					GUILayout.Space (20);
				}

				EditorGUILayout.LabelField (pair.Key.ToString(), GUILayout.Width(60));
				int w = EditorConstValue (pair.Value, 40, "weapon_weight", pair.Value);
				if (w != pair.Value)
				{
					tableConst.skillW[pair.Key] = w;
					break;
				}
				count++;
			}
			EditorGUILayout.EndHorizontal ();
			GUILayout.Space (5);
		}
		#endregion

		#region Chioce Model
		// items info
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Space (20);
		EditorGUILayout.LabelField ("Filter:", GUILayout.Width(40));
		string newFilter = EditorGUILayout.TextField (mIDFilter, GUILayout.Width(100));
		{
			mIDFilter = newFilter;
			ReadyNowList ();
		}
/*		if (GUILayout.Button (" < ", GUILayout.Width(40)))
		{
			nowModel = MoveModel (nowModel, -1);
			ReadyNowList ();
		}
		if (GUILayout.Button (" > ", GUILayout.Width(40)))
		{
			nowModel = MoveModel (nowModel, 1);
			ReadyNowList ();
		}
		IntID newModel = EditorGUILayout.IntPopup (nowModel, modelName, modelCode, GUILayout.Width(50));
		if (newModel != nowModel)
		{
			nowModel = newModel;
			ReadyNowList ();
		}*/
		EditorGUILayout.LabelField (" Model: " + nowList.Count.ToString(), GUILayout.Width(100));
		EditorGUILayout.LabelField (" All: " + tItemDict.Count.ToString(), GUILayout.Width(100));
		bool newshowAll = EditorGUILayout.ToggleLeft ("Show All", showAll);
		if (showAll != newshowAll)
		{
			showAll = newshowAll;
			ReadyNowList ();
		}
		EditorGUILayout.EndHorizontal ();
		#endregion
		#region Field Name
		EditorGUILayout.BeginHorizontal (); // ablility list
		GUILayout.Space (20);
		EditorGUILayout.LabelField ("DN        ", GUILayout.Width(25));
		EditorGUILayout.LabelField ("Insert    ", GUILayout.Width(40));
		EditorGUILayout.LabelField ("ItemID    ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("Scale     ", GUILayout.Width(30));
		EditorGUILayout.LabelField ("Skill     ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("capEnergy ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("genEnergy ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("capBody   ", GUILayout.Width(55));
		EditorGUILayout.LabelField ("capShield ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("genShield ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("capStorage", GUILayout.Width(60));
		EditorGUILayout.LabelField ("capAccel  ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("capSpeed  ", GUILayout.Width(60));
		EditorGUILayout.LabelField ("capSight  ", GUILayout.Width(60));
		//EditorGUILayout.LabelField ("gain      ", GUILayout.Width(40));
		refreshgain = GUILayout.Button ("gain", GUILayout.Width(40));
		EditorGUILayout.EndHorizontal ();
		#endregion
		#region Edit Const ablility rate
		if (showConst)
		{
			EditorGUILayout.BeginHorizontal (); // ablility list
			GUILayout.Space (20);
			if (GUILayout.Button (editConst == "" ? "" : "Cancel", GUILayout.Width(60)))
			{
				editAbillR = new DicAbilityRate (tableConst.abillR);
				editConst = "";
				EditorUtility.FocusProjectWindow(); // focus out
			}
			if (GUILayout.Button (editConst == "" ? "" : "Change: " + editConst, GUILayout.Width(167)))
			{
				EditConstRateAllItem ();
				editConst = "";
				EditorUtility.FocusProjectWindow(); // focus out
			}
			editAbillR[ItemAbility.capEnergy] = EditorConstValue (editAbillR[ItemAbility.capEnergy], 60, "capEnergy", tableConst.abillR[ItemAbility.capEnergy]);
			editAbillR[ItemAbility.genEnergy] = EditorConstValue (editAbillR[ItemAbility.genEnergy], 60, "genEnergy", tableConst.abillR[ItemAbility.genEnergy]);
			editAbillR[ItemAbility.capBody  ] = EditorConstValue (editAbillR[ItemAbility.capBody  ], 55, "capBody"  , tableConst.abillR[ItemAbility.capBody  ]);
			editAbillR[ItemAbility.capShield] = EditorConstValue (editAbillR[ItemAbility.capShield], 60, "capShield", tableConst.abillR[ItemAbility.capShield]);
			editAbillR[ItemAbility.genShield] = EditorConstValue (editAbillR[ItemAbility.genShield], 60, "genShield", tableConst.abillR[ItemAbility.genShield]);
			GUILayout.Space (60+4);
			editAbillR[ItemAbility.capAccel]  = EditorConstValue (editAbillR[ItemAbility.capAccel] , 60, "capAccel" , tableConst.abillR[ItemAbility.capAccel] );
			editAbillR[ItemAbility.capSpeed]  = EditorConstValue (editAbillR[ItemAbility.capSpeed] , 60, "capSpeed" , tableConst.abillR[ItemAbility.capSpeed] );
			EditorConstValue (0, 60, "capSight", 0);
			EditorGUILayout.EndHorizontal ();
		}
		#endregion

		#region Edit Item List
		scrollpos = EditorGUILayout.BeginScrollView (new Vector2(0, scrollpos.y));
		foreach (TableItem item in nowList)
		{
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Space (20);
			if (GUILayout.Button ("D", GUILayout.Width(25)))
			{
				newItem = item;

				tItemDict.Remove(item.itemID);

				ReadyNowList ();
			}
			if (GUILayout.Button ("+", GUILayout.Width(40)))
			{
				TableItem insert = item;
				IntID id = item.itemID;
				for (; tItemDict.ContainsKey(id+1); id++)
				{
					TableItem saveitem = tItemDict[id+1];
					insert.itemID = id+1;
					tItemDict[insert.itemID] = insert;
					insert = saveitem;
				}
				insert.itemID = id+1;
				tItemDict[insert.itemID] = insert;
				
				ReadyNowList ();
			}
			change = false;
			TableItem eitem = GUIEditor_Item (item, ref change);
			if (change)
			{
				if (eitem.itemID == item.itemID)
				{
					tItemDict[eitem.itemID] = eitem;
				}
				
				ReadyNowList ();
			}
			EditorGUILayout.EndHorizontal ();
		}
		EditorGUILayout.EndScrollView();
		#endregion

		#region Add Item
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Space (20);
		if (GUILayout.Button ("ADD", GUILayout.Width(70)))
		{
			tItemDict[newItem.itemID] = newItem;
			ReadyNowList ();
		}
		change = false;
		newItem = GUIEditor_Item (newItem, ref change, true);
		EditorGUILayout.EndHorizontal ();
		#endregion
		#region Edit ItemID
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Space (20);
		if (changeID != 0)
		{
			EditorGUILayout.LabelField (changeID.ToString(), GUILayout.Width(60));
			changeEditID = EditorGUILayout.IntField (changeEditID, GUILayout.Width(60));
			if (GUILayout.Button ("Change", GUILayout.Width(100)))
			{
				if (changeEditID == changeID)
				{
					changeID = 0;
				}
				else
				{
					if (tItemDict.ContainsKey(changeEditID) == false)
					{
						TableItem item = tItemDict[changeID];
						item.itemID = changeEditID;
						tItemDict[item.itemID] = item;
						tItemDict.Remove(changeID);
						
						ReadyNowList ();
						
						changeID = 0;
					}
				}
			}
		}
		EditorGUILayout.EndHorizontal ();
		#endregion
	}
	void Update ()
	{

	}
	#endregion
}
