using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace StardewTestMod1
{
    internal sealed class ModEntry : Mod
    {
        private Dictionary<long, PlayerInfo> _clothSets = new();
        
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.Saving += this.OnSaved;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }
        
        private void OnSaved(object? sender, SavingEventArgs e)
        {
            // Save data
            var str = ToJsonString(this._clothSets);
            Monitor.Log("Saved data: " + str, LogLevel.Debug);
            this.Helper.Data.WriteSaveData("cloth-sets", str);
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // Load data
            var data = this.Helper.Data.ReadSaveData<string>("cloth-sets");
            Monitor.Log("Loaded data: " + data, LogLevel.Debug);
            if (data != null)
            {
                this._clothSets = FromJsonString<Dictionary<long, PlayerInfo>>(data);
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            // e.Button : NumPad1 ~ NumPad9 -> int 0 ~ 8
            if (e.Button.TryGetKeyboard(out var key) && key.ToString().StartsWith("NumPad"))
            {
                int index = int.Parse(key.ToString().Substring(6)) -1;
                // check if the player has cloth set data
                if (!_clothSets.ContainsKey(Game1.player.UniqueMultiplayerID))
                {
                    var hat = Game1.player.hat.Value != null ? ToXmlString(Game1.player.hat.Value) : "";
                    var shirt = Game1.player.shirtItem.Value != null ? ToXmlString(Game1.player.shirtItem.Value) : "";
                    var pants = Game1.player.pantsItem.Value != null ? ToXmlString(Game1.player.pantsItem.Value) : "";
                    var boots = Game1.player.boots.Value != null ? ToXmlString(Game1.player.boots.Value) : "";
                    var clothSet = new ClothSet(hat, shirt, pants, boots);
                    _clothSets.Add(Game1.player.UniqueMultiplayerID, new PlayerInfo(clothSet));
                }
                var info = this._clothSets[Game1.player.UniqueMultiplayerID];
                if (info.SelectedClothSetIndex == index) return;

                if (!info.ClothSets.ContainsKey(index))
                {
                    info.ClothSets.Add(index, new ClothSet("", "", "", ""));
                }
                
                Monitor.Log("Cloth set changed to " + index, LogLevel.Debug);

                if (Game1.player.hat.Value != null)
                {
                    string hatData = ToXmlString(Game1.player.hat.Value);
                    info.ClothSets[info.SelectedClothSetIndex].Hat = hatData;
                }
                
                if (Game1.player.shirtItem.Value != null)
                {
                    string shirtData = ToXmlString(Game1.player.shirtItem.Value);
                    info.ClothSets[info.SelectedClothSetIndex].Shirt = shirtData;
                }
                
                if (Game1.player.pantsItem.Value != null)
                {
                    string pantsData = ToXmlString(Game1.player.pantsItem.Value);
                    info.ClothSets[info.SelectedClothSetIndex].Pants = pantsData;
                }
                
                if (Game1.player.boots.Value != null)
                {
                    string bootsData = ToXmlString(Game1.player.boots.Value);
                    info.ClothSets[info.SelectedClothSetIndex].Boots = bootsData;
                }

                if (info.ClothSets.ContainsKey(index))
                {
                    info.SelectedClothSetIndex = index;
                    string hatData = info.ClothSets[index].Hat;
                    if (string.IsNullOrEmpty(hatData))
                    {
                        Game1.player.hat.Value = null;
                    }
                    else
                    {
                        Game1.player.hat.Value = FromXmlString<Hat>(hatData);
                    }
                    
                    string shirtData = info.ClothSets[index].Shirt;
                    if (string.IsNullOrEmpty(shirtData))
                    {
                        Game1.player.shirtItem.Value = null;
                    }
                    else
                    {
                        Game1.player.shirtItem.Value = FromXmlString<Clothing>(shirtData);
                    }
                    
                    string pantsData = info.ClothSets[index].Pants;
                    if (string.IsNullOrEmpty(pantsData))
                    {
                        Game1.player.pantsItem.Value = null;
                    }
                    else
                    {
                        Game1.player.pantsItem.Value = FromXmlString<Clothing>(pantsData);
                    }
                    
                    string bootsData = info.ClothSets[index].Boots;
                    if (string.IsNullOrEmpty(bootsData))
                    {
                        Game1.player.boots.Value = null;
                    }
                    else
                    {
                        Game1.player.boots.Value = FromXmlString<Boots>(bootsData);
                    }

                    Game1.playSound("shwip");
                }
            }
        }

        private string ToXmlString<T>(T obj)
        {
            using (var writer = new System.IO.StringWriter())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }
        
        private T FromXmlString<T>(string xml)
        {
            using (var reader = new System.IO.StringReader(xml))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }
        
        private string ToJsonString<T>(T obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
        
        private T FromJsonString<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
    }
}
