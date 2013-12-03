namespace Auraasdf

import Boo.Lang.PatternMatching
import Boo.Lang.Compiler
import Boo.Lang.Compiler.Ast
import Boo.Lang.Compiler.MetaProgramming

class EnableResponsePatternAttribute(AbstractAstAttribute):
	override def Apply(node as Node):
		node.Accept(ResponseTransformer())
		
	class ResponseTransformer(DepthFirstTransformer):
		override def OnReferenceExpression(node as ReferenceExpression):
			if node.Name != "response": return
			enclosingStatement = node.GetAncestor of Statement()
			enclosingBlock = enclosingStatement.GetAncestor of Block()
			index = enclosingBlock.Statements.IndexOf(enclosingStatement)
			enclosingBlock.Statements.Insert(index, [| yield responseRef = Response() |])
			ReplaceCurrentNode [| responseRef.Value |]

class EnableFooPatternAttribute(AbstractAstAttribute):
	override def Apply(node as Node):
		node.Accept(ResponseTransformer())
		
	class ResponseTransformer(DepthFirstTransformer):
		override def OnReferenceExpression(node as ReferenceExpression):
			if node.Name != "Foo": return
			enclosingStatement = node.GetAncestor of Statement()
			enclosingBlock = enclosingStatement.GetAncestor of Block()
			index = enclosingBlock.Statements.IndexOf(enclosingStatement)
			#enclosingBlock.Statements.Insert(index, [| yield responseRef = Response() |])
			ReplaceCurrentNode [| print("Foobar") |]
