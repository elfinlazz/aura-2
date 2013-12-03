namespace blahtest

import System
import Aura.Channel
import Aura.Channel.Network
import Aura.Channel.Scripting.BaseScripts
import Aura.Shared.Mabi.Const
import Aura.Shared.Util
import Auraasdf

import Boo.Lang.PatternMatching
import Boo.Lang.Compiler
import Boo.Lang.Compiler.Ast
import Boo.Lang.Compiler.MetaProgramming

macro mes:
	mie = [| System.Console.WriteLine() |]
	mie.Arguments.AddRange(mes.Arguments)
	return ExpressionStatement(mie)

macro spellCheck(lang as string, body as string*):
	raise "Unknown language `${lang}`" if lang != "en-EN"
	for line in body:
		if line.Contains("bou"):
			yield [| print "line '${$line}' contains unknown word 'bou'. Did you mean 'boo'?" |]

macro repeatLines(repeatCount as int, lines as string*):
	for line in lines:
		yield [| print $line * $repeatCount |]

macro blub(asd as string):
	yield [| print "result: "+$asd |]
	
macro mes2(mes as string, body as MethodInvocationExpression*):
	mie = [| System.Console.WriteLine($mes) |]
	for x in body:
		mie.Arguments.Add(x)
	return ExpressionStatement(mie)

macro list(body as MethodInvocationExpression*):
	mie = [| List() |]
	for x in body:
		mie.Arguments.Add(x)
	return ExpressionStatement(mie)

macro button(name as string):
	yield [| Button($name) |]

class Button:
	private _name as string
	def constructor(name as string):
		_name = name
	def ToString():
		return "<button>"+ _name + "</button>";

class List:
	private _name as string
	def constructor(*name as (Button)):
		_name = name.ToString()
	def ToString():
		return "<button>"+ _name + "</button>";

class Npc1Script(NpcScript):
	def Button(name as string) as Button:
		return blahtest.Button(name)
	#def List(*buttons as (Button)) as string:
	#	result = ""
	#	for b in buttons:
	#		result += b.ToString()
	#	return result

	def Load():
		NPC.SetLocation(1, 13000, 38200);
		mes "asd"
		#mes("asd")
		
		#test """
		#test
		#""", "1"

		c = "{0} : {1}"
		mes "blub 2", 123
		#repeatLines 2,"Hello boo!","How do you bou?"
		
		#phrases:
		#	"Hello there."
		#	"Come with me... I shall show you a future in ruins."
		#	"E = mc² actually stands for Enjoyment = (Modifying the Client)²"
		#	"Hmm... I haven't checked in with FIONA lately..."
		#	"My latest creation is almost ready!"
		#	1

		blub "test"
		Console.WriteLine("dasdasd")

		mes2 "ext mes test{0}":
			list:
				button "Yes"; button "No"

		#if Foo == "asd":
		#	pass
		
	def test(*args as (object)):
		for arg in args:
			print arg.ToString().Trim();


[assembly: EnableResponsePatternAttribute]
[assembly: EnableFooPatternAttribute]
