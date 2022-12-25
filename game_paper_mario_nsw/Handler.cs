﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Cetera.Font;
using game_pm_nsw.Properties;
using Kontract.Compression;
using Kontract.Interface;

namespace game_pm_nsw
{
    // A class for text formatting settings for the preview
    public class TextPreviewFormat
    {
        public float offsetX;
        public float offsetY;
        public float scale;
        public float maxWidth;
        public float widthMultiplier;
        public float marginX;
        public float marginY;

        // Set some defaults for now
        public TextPreviewFormat()
        {
            offsetX = 20;
            offsetY = 10;
            scale = 0.9f;
            marginX = 25.0f;
            marginY = 25.0f;
            maxWidth = 400 - marginX * 2;
            widthMultiplier = 1;
        }
    };

    public class Handler : IGameHandler
    {
        static Lazy<BCFNT[]> fontInitializer = new Lazy<BCFNT[]>(() => new[] {
                new BCFNT(new MemoryStream(GZip.Decompress(new MemoryStream(Resources.FontCaptionOutline_bcfnt)))),
                new BCFNT(new MemoryStream(GZip.Decompress(new MemoryStream(Resources.FontCaptionOutline_bcfnt))))
            });

        BCFNT baseFont => fontInitializer.Value[0];
        BCFNT outlineFont => fontInitializer.Value[1];

        public TextPreviewFormat txtPreview;

        static Dictionary<string, string> codeLabelPair = new Dictionary<string, string>
        {
            ["<n3.0:00-CD>"] = "<miiname0>",
            ["<n3.0:01-CD>"] = "<miiname1>",
            ["<n3.0:02-CD>"] = "<miiname2>",
            ["<n3.0:03-CD>"] = "<miiname3>",
            ["<n3.1:04-CD>"] = "<miiname4>",
            ["<n3.1:05-CD>"] = "<miiname5>",
            ["<n3.1:06-CD>"] = "<miiname6>",
            ["<3.1:04-CD>"] = "<darklord>",
            ["<n3.2:01-CD>"] = "<fperson>",
            ["<n9.0:00-CD>"] = "<weapon>",
            ["<n10.0:00-CD>"] = "<armor>",
            ["<n2.0:00-06>"] = "<#gold>",
            ["<n2.0:01-06>"] = "<#needgold>",
            ["<n3.0:04-CD>"] = "<heroname1>",
            ["<n3.0:05-CD>"] = "<heroname2>",
            ["<n3.0:05-CD>"] = "<heroname2>",
            ["<n0.1:04-00>"] = "<mariofont>",
            ["<n0.F:03-04>"] = "<disabletags>",
            ["<n0.4:>"] = "<next>",
            ["<n3.40:>"] = "<wait>",
            ["<n3.0:>"] = "<5>",
            ["<n5.5:>"] = "<town>",
            ["<n3.15:>"] = "<4>",
            ["<n7.0:10-00-00-00>"] = "<3>",
            ["<n3.37:>"] = "<2>",
            ["<n7.0:08-00-00-00>"] = "<1>",
            ["<n7.0:14-00-00-00>"] = "<6>",
            ["<n3.16:>"] = "<0>",
            ["<n3.0:00-00-00-00>"] = "<number>",
            ["«"] = "<<",
            ["»"] = ">>",
            ["<n3.4>"] = "<shadow>",
            ["<n0.2:64-00>"] = "<size100>",
            ["<n0.2:41-00>"] = "<size65>",
            ["<n0.2:01-00>"] = "<size1>",
            ["<n0.2:02-00>"] = "<size2>",
            ["<n0.2:03-00>"] = "<size3>",
            ["<n0.2:04-00>"] = "<size4>",
            ["<n0.2:05-00>"] = "<size5>",
            ["<n0.2:06-00>"] = "<size6>",
            ["<n0.2:07-00>"] = "<size7>",
            ["<n0.2:08-00>"] = "<size8>",
            ["<n0.2:09-00>"] = "<size9>",
            ["<n0.2:0A-00>"] = "<size10>",
            ["<n0.2:0B-00>"] = "<size11>",
            ["<n0.2:0C-00>"] = "<size12>",
            ["<n0.2:0D-00>"] = "<size13>",
            ["<n0.2:0E-00>"] = "<size14>",
            ["<n0.2:0F-00>"] = "<size15>",
            ["<n0.2:10-00>"] = "<size16>",
            ["<n0.2:11-00>"] = "<size17>",
            ["<n0.2:12-00>"] = "<size18>",
            ["<n0.2:13-00>"] = "<size19>",
            ["<n0.1:FF-FF"] = "<popjoy>",
            ["<n0.4:><n3.0:>"] = "<new>",
            ["<n14.0:01-CD-02-00-32-00>"] = "<bubbleW>",
            ["<n20.0:>"] = "<capital>",
            ["<n14.0:02-CD-00-00>"] = "<exclaimW>",
            ["<n21.0:01-CD-04-00-3E-04-3B-04-04-00-3B-04-30-04>"] = "<gender01>",
            ["<n21.0:03-CD-02-00-42-04-06-00-48-04-3A-04-30-04>"] = "<gender03>",
            ["<n21.0:00-CD-00-00-02-00-30-04>"] = "<gender02>",
            ["<n21.0:02-CD-00-00-04-00-3B-04-30-04>"] = "<gender04>",
            ["<n21.0:03-CD-00-00-02-00-30-04>"] = "<gender06>",
            ["<n21.0:03-CD-04-00-4B-04-3C-04-04-00-3E-04-39-04>"] = "<gender07>",
            ["<n21.0:03-CD-00-00-02-00-30-04>"] = "<gender08>",
            ["<n14.0:1E-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-4B-00-69-00-6E-00-67-00-30-00-30-00>"] = "<king>",
            ["<n14.0:26-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-4E-00-6F-00-62-00-6C-00-65-00-4B-00-69-00-64-00-30-00-30-00>"] = "<noble>",
            ["<n14.0:26-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-50-00-72-00-69-00-6E-00-63-00-65-00-73-00-73-00-30-00-30-00>"] = "<princess>",
            ["<n14.0:22-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-50-00-72-00-69-00-6E-00-63-00-65-00-30-00-30-00>"] = "<prince>",
            ["<n14.0:18-00-54-00-6F-00-77-00-6E-00-30-00-31-00-5F-00-47-00-65-00-6E-00-69-00-65-00>"] = "<genie>",
            ["<n14.0:24-00-54-00-6F-00-77-00-6E-00-30-00-32-00-5F-00-53-00-69-00-73-00-74-00-65-00-72-00-46-00-69-00-72-00-73-00-74-00>"] = "<yellowelf>",
            ["<n14.0:26-00-54-00-6F-00-77-00-6E-00-30-00-32-00-5F-00-53-00-69-00-73-00-74-00-65-00-72-00-53-00-65-00-63-00-6F-00-6E-00-64-00>"] = "<redelf>",
            ["<n14.0:24-00-54-00-6F-00-77-00-6E-00-30-00-32-00-5F-00-53-00-69-00-73-00-74-00-65-00-72-00-54-00-68-00-69-00-72-00-64-00>"] = "<purpleelf>",
            ["<n14.0:0A-00-53-00-61-00-74-00-61-00-6E-00>"] = "<satan>",
            ["<n14.0:08-00-53-00-61-00-67-00-65-00>"] = "<sage>",
            ["<n14.0:1A-00-52-00-65-00-69-00-6E-00-63-00-61-00-72-00-6E-00-61-00-74-00-69-00-6F-00-6E-00>"] = "<reincarn>",
        };

        // ruby code ID, appended to the label for each new ruby code
        // this value is incremented after a new (key, value) pair is added to the dictionary
        int rubyCodeID = 0;

        public string Name { get; } = "Paper Mario: The Origami King";

        // Displaying the text
        public string GetKuriimuString(string str)
        {
            try
            {
                Func<string, byte[], string> Fix = (id, bytes) =>
                {
                    return $"n{(int)id[0]}.{(int)id[1]}:" + BitConverter.ToString(bytes);
                };

                int i;
                while ((i = str.IndexOf('\xE')) >= 0)
                {
                    var id = str.Substring(i + 1, 2);
                    var data = str.Substring(i + 4, str[i + 3]).Select(c => (byte)c).ToArray();

                    if (id == "\0\0")
                    {
                        // Replace the ruby code with labels <rubyN>, where "N" is 0, 1, 2....
                        string key = $"<{Fix(id, data)}>";
                        string value = $"<ruby{rubyCodeID}>";

                        // Add the new (key, value) pairs for the ruby code
                        if (!codeLabelPair.ContainsKey(key))
                        {
                            codeLabelPair.Add(key, value);
                            ++rubyCodeID;
                        }

                        str = str.Remove(i, data.Length + 4).Insert(i, key);
                    }
                    else
                    {
                        str = str.Remove(i, data.Length + 4).Insert(i, $"<{Fix(id, data)}>");
                    }
                }

                str = codeLabelPair.Aggregate(str, (s, pair) => s.Replace(pair.Key, pair.Value));
                return AutiFitLine(str,33);
            }
            catch
            {
                return AutiFitLine(str, 33);
            }
        }
        public static string AutiFitLine(string text, int maxLength)
        {
            string[] splitted = text.Split();
            string result = "";
            int symbols = 0;
            int line_count = 0;
            foreach (var word in splitted)
            {
                if (word.Length + symbols > maxLength)
                {
                    result += "\n";
                    symbols = word.Length;
                    result += word;
                    line_count++;
                }
                else
                {
                    if (symbols > 0)
                    {
                        result += " ";
                        symbols++;
                    }
                    result += word;
                    symbols += word.Length;
                }
            }
            return result;
        }
        public string GetRawString(string str)
        {
            try
            {
                if (str.Length < 3)
                {
                    return str;
                }

                str = codeLabelPair.Aggregate(str, (s, pair) => s.Replace(pair.Value, pair.Key));
                string result = string.Empty;
                result = string.Concat(str.Split("<>".ToArray()).Select((codeString, i) =>
                {
                    // codeString = "n00:code"
                    if (i % 2 == 0)
                    {
                        return codeString;
                    }

                    // "0.0:code" part, the identyfier ("n") infront of 0.0 is stripped
                    var codeStringRaw = codeString.Substring(1);

                    // separate the code id "00.00" and the hex code "00-00""
                    string[] codeStringArray = codeStringRaw.Split(':');

                    // get the ID part
                    string[] idString = codeStringArray[0].Split('.');

                    // get the hex string with the ID ("X.X") part stripped
                    string hexString = codeStringArray[1];

                    if (hexString.Length > 0)
                    {
                        Func<string, byte[], string> Merge = (id, data) => $"\xE{id}{(char)data.Length}{string.Concat(data.Select(b => (char)b))}";

                        byte[] byteArray = hexString.Split('-').Select(piece => Convert.ToByte(piece, 16)).ToArray();

                        string idHex = "" + (char)int.Parse(idString[0]) +
                                            (char)int.Parse(idString[1]);
                        return Merge(idHex, byteArray);
                    }
                    else
                    {
                        Func<string, int, string> MergeEmpty = (id, length) => $"\xE{id}{(char)length}";

                        string idHex = "" + (char)int.Parse(idString[0]) +
                                            (char)int.Parse(idString[1]);

                        return MergeEmpty(idHex, 0);
                    }

                }));

                return result;
            }
            catch
            {
                return str;
            }
        }

        public bool HandlerCanGeneratePreviews { get; } = true;

        public bool HandlerHasSettings { get; } = false;

        // Show the settings dialog
        public bool ShowSettings(Icon icon)
        {
            return false;
        }

        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry?.EditedText == null) return pages;

            string labelString = GetKuriimuString(entry.EditedText);
            if (string.IsNullOrWhiteSpace(labelString))
            {
                labelString = entry.OriginalText;
            }

            Bitmap backgroundImg = new Bitmap(Resources.previewbg, 384, 128);

            // gold FromArgb(218, 165, 32)
            baseFont.SetColor(Color.FromArgb(60, 61, 56));
            outlineFont.SetColor(Color.Black);

            // create default preview settings
            txtPreview = new TextPreviewFormat();

            using (var g = Graphics.FromImage(backgroundImg))
            {
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.Bicubic;

                float x = 0;
                float y = -5;

                for (int i = 0; i < labelString.Length; ++i)
                {
                    var c = labelString[i];

                    var charWidth = baseFont.GetWidthInfo(c).char_width * txtPreview.scale * txtPreview.widthMultiplier;
                    if (c == '\n' || (x + charWidth >= txtPreview.maxWidth))
                    {
                        x = 0;
                        y += baseFont.LineFeed * txtPreview.scale;
                        if (c == '\n')
                        {
                            continue;
                        }
                    }

                    baseFont.Draw(c, g, x + txtPreview.offsetX + txtPreview.marginX, y + txtPreview.offsetY + txtPreview.marginY,
                                    txtPreview.scale * txtPreview.widthMultiplier, txtPreview.scale);

                    x += charWidth;
                }
            }

            pages.Add(backgroundImg);
            return pages;
        }

        public Image Icon
        {
            get { return Resources.icon_2; }
        }
    }
}
