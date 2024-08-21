grammar Lang;

expr:
    | expr OR expr
    | expr AND expr
    | expr ('=' | '!=') expr
    | expr ('<=' | '>=' | '<' | '>') expr
	| expr ('+' | '-') expr
	| expr ('*' | '/') expr
	| LEFT_PARENTHESIS expr RIGHT_PARENTHESIS
	| string
	| name
	| func
	| integer
	| number
	| boolean
	| null
	;
	
string: STRING;

name: NAME | BLOCKED_NAME;

integer: INTEGER;

boolean: BOOLEAN;

null: NULL;

number: NUMBER;

func:
	| NAME LEFT_PARENTHESIS RIGHT_PARENTHESIS
	| NAME LEFT_PARENTHESIS expr (',' expr)* RIGHT_PARENTHESIS;

AND: [a] [n] [d];
OR: [o] [r];
LEFT_PARENTHESIS: '(';
RIGHT_PARENTHESIS: ')';
PLUS: '+';
MINUS: '-';
MUL: '*';
DIV: '/';
LESS_EQUAL: '<=';
LESS_THEN: '<';
GREATER_EQUAL: '>=';
GREATER_THEN: '>';

NAME: [a-zA-Z_][a-zA-Z_0-9]*;

BLOCKED_NAME: '[' (' ')* [a-z-A-Z_] ([a-zA-Z_0-9] | ' ')* ']';

STRING: ['] (~['\r\n])* ['];

NEWLINE: ('\r'? '\n' | '\r')+;

INTEGER: [0-9]+;

NUMBER: [0-9]+ '.' [0-9]+;

BOOLEAN: 'true' | 'false';

NULL: 'null';

WS: (' ' | '\t')* -> skip;