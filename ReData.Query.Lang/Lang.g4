grammar Lang;

start:
    expr EOF;

expr
    : MINUS expr #unary
    | <assoc=right> expr (HAT) expr #binary
    | expr (MUL | DIV) expr #binary
	| expr (PLUS | MINUS) expr #binary
    | expr (LESS_THEN | LESS_EQUAL | GREATER_THEN | GREATER_EQUAL) expr #binary
    | expr (EQUAL | NOT_EQUAL) expr #binary
    | expr AND expr #binary
    | expr OR expr #binary
    | LEFT_PARENTHESIS expr RIGHT_PARENTHESIS #scope
	| string #literal
	| boolean #literal
	| null #literal
	| name #literal
	| func #function
	| integer #literal
	| number #literal
	| expr DOT NAME LEFT_PARENTHESIS (expr (',' expr)*)? RIGHT_PARENTHESIS #objectFunction
	;
	
string: STRING;

null: NULL;

boolean: BOOLEAN;

name: NAME | BLOCKED_NAME;

integer: INTEGER;

number: NUMBER;

func : NAME LEFT_PARENTHESIS (expr (',' expr)*)? RIGHT_PARENTHESIS;

AND: [a] [n] [d];
OR: [o] [r];
LEFT_PARENTHESIS: '(';
RIGHT_PARENTHESIS: ')';
HAT: '^';
PLUS: '+';
MINUS: '-';
MUL: '*';
DIV: '/';
LESS_EQUAL: '<=';
LESS_THEN: '<';
GREATER_EQUAL: '>=';
GREATER_THEN: '>';
EQUAL: '=';
NOT_EQUAL: '!=';
DOT: '.';

BOOLEAN: 'true' | 'false';

NULL: 'null';

NAME: [a-zA-Z_][a-zA-Z_0-9]*;

BLOCKED_NAME: '[' (' ')* [a-z-A-Z_] ([a-zA-Z_0-9] | ' ')* ']';

STRING: ['] (~['\r\n])* ['];

INTEGER: [0-9]+;

NUMBER: ([0-9]* '.' [0-9]+) | ([0-9]+ '.' [0-9]+);

WS: [ \t\r\n]+ -> skip;