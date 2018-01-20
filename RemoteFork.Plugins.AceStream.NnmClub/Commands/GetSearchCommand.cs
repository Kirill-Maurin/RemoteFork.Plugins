﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using RemoteFork.Network;

namespace RemoteFork.Plugins.Commands {
    public class GetSearchCommand : ICommand {
        public List<Item> GetItems(IPluginContext context = null, params string[] data) {
            string responseFromServer = HTTPUtility.PostRequest(NnmClub.TrackerServerNnm + "/forum/tracker.php", $"nm={data[2]}");

            var items = new List<Item>();
            var regex = new Regex("(<tr class=\"prow)([\\s\\S]*?)(<\\/tr>)");
            var result = regex.Matches(responseFromServer);

            if (result.Count > 0) {
                foreach (Match match in result) {
                    regex = new Regex("(<a class=\"genmed \\w*\" href=\")(.*?)(\")");
                    var item = new Item {
                        Link =
                            $"pagefilm{NnmClub.SEPARATOR}{NnmClub.TrackerServerNnm}/forum/{regex.Match(match.Value).Groups[2].Value}"
                    };
                    regex = new Regex("(\"><b>)(.*?)(<\\/b><\\/a>)");
                    item.Name = regex.Match(match.Value).Groups[2].Value;
                    item.ImageLink = NnmClub.ICO_TORRENT_FILE;
                    item.Description = GetDescriptionSearhNnm(match.Value);
                    items.Add(item);
                }
            } else {
                items = NonSearch();
            }

            NnmClub.IsIptv = false;
            return items;
        }

        private static string GetDescriptionSearhNnm(string html) {
            string nameFilm = string.Empty;
            string sizeFile = string.Empty;
            string dobavlenFile = string.Empty;
            string leechers = string.Empty;
            string seeders = string.Empty;

            var regex = new Regex("(\"><b>)(.*?)(</b>)");

            if (regex.IsMatch(html)) {
                nameFilm = regex.Match(html).Groups[2].Value;
            }
            regex = new Regex("(?<=</u>).*?(?=</td>)");
            if (regex.IsMatch(html)) {
                sizeFile = "<p> Размер: <b>" + regex.Matches(html)[0].Value + "</b>";
                dobavlenFile = "<p> Добавлен: <b>" + regex.Matches(html)[1].Value.Replace("<br>", " ") + "</b>";
            }
            regex = new Regex("(class=\"seedmed\">)(.*?)(</td>)");
            if (regex.IsMatch(html)) {
                seeders = "<p> Seeders: <b> " + regex.Match(html).Groups[2].Value + "</b>";
            }
            regex = new Regex("(ass=\"leechmed\">)(.*?)(</td>)");
            if (regex.IsMatch(html)) {
                leechers = "<p> Leechers: <b> " + regex.Match(html).Groups[2].Value + "</b>";
            }
            return "<html><font face=\"Arial\" size=\"5\"><b>" + nameFilm +
                   "</font></b><p><font face=\"Arial Narrow\" size=\"4\">" + sizeFile + dobavlenFile + seeders +
                   leechers + "</font></html>";
        }

        private static List<Item> NonSearch(bool category = false) {
            var items = new List<Item>();
            var item = new Item {
                Link = string.Empty,
                ImageLink = NnmClub.ICO_NOFILE
            };
            if (category) {
                item.Name = "<span style=\"color#F68648\">" + " - Здесь ничего нет - " + "</span>";
                item.Description = "Нет информации для отображения";
            } else {
                item.Name = "<span style=\"color#F68648\">" + " - Ничего не найдено - " + "</span>";
                item.Description = "Поиск не дал результатов";
            }

            items.Add(item);

            return items;
        }
    }
}
