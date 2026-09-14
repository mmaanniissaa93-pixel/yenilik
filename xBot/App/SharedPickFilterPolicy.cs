using System.Linq;
using Newtonsoft.Json.Linq;

namespace xBot.App
{
    public static class SharedPickFilterPolicy
    {
        public static JObject Merge(JObject local, JObject shared)
        {
            var result = (JObject)local.DeepClone();
            var rules = result["Rules"] as JArray;
            if (rules == null) { rules = new JArray(); result["Rules"] = rules; }
            // Remove old pickup choices, but preserve per-character selling/storage rules.
            foreach (JObject rule in rules.OfType<JObject>()) { rule["Pickup"] = false; rule["Pet"] = false; }
            foreach (JObject source in (shared["Rules"] as JArray ?? new JArray()).OfType<JObject>())
            {
                var target = rules.OfType<JObject>().FirstOrDefault(r => (string)r["Name"] == (string)source["Name"]
                    && (int?)r["MatchType"] == (int?)source["MatchType"] && (string)r["Pattern"] == (string)source["Pattern"]);
                if (target == null)
                {
                    target = new JObject { ["Name"] = source["Name"]?.DeepClone(), ["MatchType"] = source["MatchType"]?.DeepClone(),
                        ["Pattern"] = source["Pattern"]?.DeepClone(), ["Sell"] = false, ["Store"] = false, ["StoreGuild"] = false,
                        ["TakeStorage"] = false, ["TakeGuildStorage"] = false };
                    rules.Add(target);
                }
                target["Pickup"] = source["Pickup"]?.DeepClone(); target["Pet"] = source["Pet"]?.DeepClone();
            }
            var options = result["PickOptions"] as JObject;
            if (options == null) { options = new JObject(); result["PickOptions"] = options; }
            if (shared["PickOptions"] is JObject input)
                foreach (var property in input.Properties())
                    if (property.Name.StartsWith("Pick") || property.Name.StartsWith("DontPick")
                        || property.Name.StartsWith("OnlyPick") || property.Name == "UsePickPet" || property.Name == "Enabled"
                        || property.Name == "ArrowBoltAmount") options[property.Name] = property.Value.DeepClone();
            foreach (string key in new[] { "MinDegree", "MaxDegree", "OnlySox", "FilterChina", "FilterEurope", "FilterMale", "FilterFemale" })
                if (shared[key] != null) result[key] = shared[key].DeepClone();
            return result;
        }

        public static JObject MergeCategories(JObject local, JObject shared)
        {
            var result = (JObject)local.DeepClone();
            foreach (var property in shared.Properties())
            {
                var target = result[property.Name] as JObject;
                if (target == null) { target = new JObject(); result[property.Name] = target; }
                target["Pick"] = property.Value["Pick"]?.DeepClone();
                target["PetPick"] = property.Value["PetPick"]?.DeepClone();
            }
            return result;
        }
    }
}
