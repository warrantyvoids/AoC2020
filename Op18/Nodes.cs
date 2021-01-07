namespace Op18
{
	abstract public class AstNode
	{
		public abstract ulong Value { get; }
	}
	
	public sealed class AstConstant : AstNode
	{
		public AstConstant(ulong value)
		{
			Value = value;
		}

		public override ulong Value { get; }
	}

	public sealed class AstAdd : AstNode
	{
		public AstNode Lhs { get; }
		public AstNode Rhs { get; }

		public AstAdd(AstNode lhs, AstNode rhs)
		{
			Lhs = lhs;
			Rhs = rhs;
		}

		public override ulong Value => Lhs.Value + Rhs.Value;
	}

	public sealed class AstMultiply : AstNode
	{
		public AstNode Lhs { get; }
		public AstNode Rhs { get; }

		public AstMultiply(AstNode lhs, AstNode rhs)
		{
			Lhs = lhs;
			Rhs = rhs;
		}

		public override ulong Value => Lhs.Value * Rhs.Value;
	}

	public sealed class AstParenthesis : AstNode
	{
		public AstNode Body { get; }

		public AstParenthesis(AstNode body)
		{
			Body = body;
		}

		public override ulong Value => Body.Value;
	}
}