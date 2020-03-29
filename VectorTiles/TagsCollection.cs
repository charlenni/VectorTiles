using System.Collections.Generic;
using System.Text;

namespace VectorTiles
{
    /// <summary>
    /// Represents a simple tags collection based on a dictionary.
    /// </summary>
    public class TagsCollection
    {
        private const char KeyValueSeparator = '=';

        private List<string> keys = new List<string>();
        private List<object> values = new List<object>();

        /// <summary>
        /// Creates a new tags collection.
        /// </summary>
        public TagsCollection(params KeyValuePair<string, object>[] tags)
        {
            foreach (var tag in tags)
                Add(tag.Key, tag.Value);
        }

        /// <summary>
        /// Creates a new tags collection initialized with the given existing tags.
        /// </summary>
        /// <param name="tags"></param>
        public TagsCollection(IEnumerable<KeyValuePair<string, object>> tags)
        {
            foreach (var tag in tags)
                Add(tag.Key, tag.Value);
        }

        /// <summary>
        /// Creates a new tags collection initialized with the given existing tags.
        /// </summary>
        /// <param name="tags"></param>
        public TagsCollection(IDictionary<string, string> tags)
        {
            if (tags != null)
            {
                foreach (KeyValuePair<string, string> pair in tags)
                {
                    Add(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// Number of elements in this tags collection
        /// </summary>
        public int Count { get => keys.Count; }

        public object this[string key]
        {
            get
            {
                int index = keys.IndexOf(key);
                if (index < 0)
                    return null;
                return values[index];
            }
            set
            {
                Add(key, value);
            }
        }

        public IEnumerable<KeyValuePair<string, object>> KeyValues
        {
            get
            {
                var result = new List<KeyValuePair<string, object>>(Count);

                for (int i = 0; i < keys.Count; i++)
                    result.Add(new KeyValuePair<string, object>(keys[i], values[i]));

                return result;
            }
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        /// <summary>
        /// Adds a tag from a string with key-value-separator
        /// </summary>
        /// <param name="tag">String of key-value-pair separated with key-value-separator</param>
        public void Add(string tag)
        {
            var splitPosition = tag.IndexOf(KeyValueSeparator);

            Add(tag.Substring(0, splitPosition), tag.Substring(splitPosition + 1));
        }

        /// <summary>
        /// Adds a list of tags to this collection.
        /// </summary>
        /// <param name="tags">List of tags</param>
        public void Add(IEnumerable<KeyValuePair<string, object>> tags)
        {
            foreach (var tag in tags)
                Add(tag.Key, tag.Value);
        }

        /// <summary>
        /// Adds a list of tags to this collection.
        /// </summary>
        /// <param name="tags">List of tags</param>
        public void Add(TagsCollection tags)
        {
            foreach (var tag in tags.KeyValues)
                Add(tag.Key, tag.Value);
        }

        /// <summary>
        /// Add a key value pair to the TagsCollection
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, object value)
        {
            int index = keys.IndexOf(key);

            if (index < 0)
            {
                keys.Add(key);
                values.Add(value);
            }
            else
            {
                values[index] = value;
            }
        }

        /// <summary>
        /// Returns true, if the tags collection contains given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return keys.Contains(key);
        }

        /// <summary>
        /// Returns true if the given key-value pair is found in this tags collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsKeyValue(string key, object value)
        {
            int index = keys.IndexOf(key);

            if (index < 0 || values[index] == null)
                return false;

            return values[index].Equals(value);
        }

        /// <summary>
        /// Returns true if one of the given keys exists in this tag collection.
        /// </summary>
        /// <param name="keys">Collection of keys to check</param>
        /// <returns>True, if one key in keys is containd in this collection</returns>
        public virtual bool ContainsOneOfKeys(ICollection<string> others)
        {
            foreach (var other in others)
            {
                if (keys.Contains(other))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes all tags with given key and value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RemoveKeyValue(string key, object value)
        {
            if (ContainsKeyValue(key, value))
            {
                int index = keys.IndexOf(key);
                keys.RemoveAt(index);
                values.RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check for equality
        /// </summary>
        /// <param name="other">Other TagCollections to check</param>
        /// <returns>True, if it is the same TagsCollection or contains the same tags</returns>
        public override bool Equals(object other)
        {
            if (other == this)
                return true;

            if (!(other is TagsCollection otherTagsCollection) || otherTagsCollection.Count != keys.Count)
                return false;

            for (int i = 0; i < keys.Count; i++)
                if (!otherTagsCollection.ContainsKeyValue(keys[i], values[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns a string that represents this tags collection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder tags = new StringBuilder();
            for (int i = 0; i < keys.Count; i++)
            {
                tags.Append($"{keys[i]}{KeyValueSeparator}{values[i]}");
                tags.Append(',');
            }
            if (tags.Length > 0)
            {
                return tags.ToString(0, tags.Length - 1);
            }
            return "empty";
        }
    }
}