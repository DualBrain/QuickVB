# Boolean Operators

Boolean operators perform bit manipulations, Boolean operations, or tests on multiple relations. They return a true (nonzero) or false (zero) value to be used in making a decision.

## Syntax

*result* = *expression1* **boolean-operator** *expression2*

## Comments

*boolean-operator* is any of the following:

| --- | --- |
| NOT | Bit-wise complement |
| AND | Conjunction |
| OR | Disjunction (inclusive 'or') |
| XOR | Exclusive 'or' |
| EQV | Equivalence |
| IMP | Implication |

Each operator returns results as indicated in the following truth table. *T* is true (nonzero) and *F* is false (zero).

| *expression1* | *expression2* | NOT | AND | OR | XOR | EQV | IMP |
| --- | --- | --- | --- | --- | --- | --- | --- |
| T | T | F | T | T | F | T | T |
| T | F | F | F | T | T | F | F |
| F | T | T | F | T | T | F | T |
| F | F | T | F | F | F | T | T |

Boolean operations are performed after arithmetic and relational operations in order of precedence.

Expressions are converted to integers or long integers before a Boolean operation is performed.

If the expressions evaluate to 0 or -1, a Boolean operation returns 0 or -1 as the result. Because Boolean operators do bit-wise calculations, using values other than 0 for false and -1 for true may produce unexpected results.
