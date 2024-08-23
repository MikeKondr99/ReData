grammar Lang;

expr:
	| expr ('*' | '/') expr
	| expr ('+' | '-') expr
    | expr ('<=' | '>=' | '<' | '>') expr
    | expr ('=' | '!=') expr
    | expr AND expr
    | expr OR expr
	| LEFT_PARENTHESIS expr RIGHT_PARENTHESIS
	| string
	| boolean
	| null
	| name
	| func
	| integer
	| number
	;
	
string: STRING;

null: NULL;

boolean: BOOLEAN;

name: NAME | BLOCKED_NAME;

integer: INTEGER;



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

BOOLEAN: 'true' | 'false';

NULL: 'null';

NAME: [a-zA-Z_][a-zA-Z_0-9]*;

BLOCKED_NAME: '[' (' ')* [a-z-A-Z_] ([a-zA-Z_0-9] | ' ')* ']';

STRING: ['] (~['\r\n])* ['];

NEWLINE: ('\r'? '\n' | '\r')+;

INTEGER: [0-9]+;

NUMBER: [0-9]+ '.' [0-9]+;

WS: (' ' | '\t')* -> skip;