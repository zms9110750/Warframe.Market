namespace Warframe.Market.Helper;

public class TrieNode
{
	public string? Word { get; private set; } // 存储完整单词
	private Dictionary<char, TrieNode> Children { get; set; } = new();

	// 对外暴露的 Add 方法，只接受 string
	public void Add(string word)
	{
		if (string.IsNullOrEmpty(word))
			throw new ArgumentException("Word cannot be null or empty.", nameof(word));

		AddInternal(word.AsSpan(), word);
	}

	// 内部使用的 Add 方法，接受 ReadOnlySpan<char> 和 string
	private void AddInternal(ReadOnlySpan<char> chars, string word)
	{
		// 如果字符为空，存储完整单词
		if (chars.Length == 0)
		{
			Word = word;
			return;
		}

		var firstChar = chars[0];

		// 如果字符是空格，则压缩连续的空格
		if (firstChar == ' ')
		{
			// 跳过后续的空格
			var trimmedChars = chars.TrimStart(' ');
			if (trimmedChars.Length > 0)
			{
				if (!Children.TryGetValue(' ', out var spaceNode))
				{
					spaceNode = new TrieNode();
					Children[' '] = spaceNode;
				}
				// 递归插入剩余字符
				spaceNode.AddInternal(trimmedChars, word);
			}
			return;
		}

		// 处理非空格字符
		if (!Children.TryGetValue(firstChar, out var childNode))
		{
			childNode = new TrieNode();
			Children[firstChar] = childNode;
		}

		// 递归插入剩余字符
		childNode.AddInternal(chars.Slice(1), word);
	}
	// 搜索匹配的单词
	public IEnumerable<string> Search(ReadOnlyMemory<char> chars)
	{
		switch (chars)
		{
			// 如果字符为空
			case { Span: [] }:
				// 如果当前节点存储了单词，则返回该单词
				IEnumerable<string>? currentWord = Word != null ? [Word] : Enumerable.Empty<string>();

				// 返回所有子节点的匹配结果
				var childResults = Children.Values.SelectMany(childNode => childNode.Search(chars));

				// 连接当前单词和子节点结果
				return currentWord.Concat(childResults);

			// 如果字符以空格开头
			case { Span: [' ', ..] }:
				// 截断空格并递归匹配剩余字符
				var spaceNodeResults = Children.TryGetValue(' ', out var spaceNode)
					? spaceNode.Search(chars.TrimStart(' '))
					: Enumerable.Empty<string>();

				// 继续在当前节点的子节点中匹配完整字符（不跳过空格）
				var fullCharResults = Children.Values.SelectMany(child => child.Search(chars));

				// 连接空格节点结果和完整字符结果
				return spaceNodeResults.Concat(fullCharResults);

			// 如果字符以非空格开头
			case { Span: [var firstChar, ..] } when Children.TryGetValue(firstChar, out var childNode):
				// 递归匹配剩余字符
				return childNode.Search(chars.Slice(1));

			// 默认情况
			default:
				return Enumerable.Empty<string>();
		}
	}
}
