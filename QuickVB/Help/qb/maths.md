# Appendix B

## Mathematical Functions

Mathematical functions not intrinsic to BASIC can be calculated as follows:

Function | BASIC Equivalent
-- | --
Secant | SEC(X)=1/COS(X)
Cosecant | CSC(X)=1/SIN(X)
Cotangent | COT(X)=1/TAN(X)
Inverse Sine | ARCSIN(X)=ATN(X/SQR(-X*X+1))
Inverse Cosine | ARCCOS(X)=ATN (X/SQR(-X*X+1))+ PI/2
Inverse Secant | ARCSEC(X)=ATN(X/SQR(X*X-1))+SGN(SGN(X)-1)* PI/2
Inverse Cosecant | ARCCSC(X)=ATN(X/SQR(X*X-1))+SGN(X)-1)* PI/2
Inverse Cotangent | ARCCOT(X)=ATN(X)+ PI/2
Hyperbolic Sine | SINH(X)=(EXP(X)-EXP(-X))/2
Hyperbolic Cosine | COSH(X)=(EXP(X)+EXP(-X))/2
Hyperbolic Tangent | TANH(X)=EXP(X)-EXP(-X))/+(EXP(X)+EXP(-X))
Hyperbolic Secant | SECH(X)=2/(EXP(X)+EXP(-X))
Hyperbolic Cosecant | CSCH(X)=2/(EXP(X)-EXP(-X))
Hyperbolic Cotangent | COTH(X)=EXP(-X)/(EXP(X)-EXP(-X))*2+1
Inverse Hyperbolic Sine | ARCSINH(X)=LOG(X/SQR(X*X+1))
Inverse Hyperbolic Cosine | ARCCOSH(X)=LOG(X+SQR(X*X-1))
Inverse Hyperbolic Tangent | ARCTANH(X)=LOG((1+X)/(1-X))/2
Inverse Hyperbolic Cosecant | ARCCSCH(X)=LOG(SGN(X)*SQR(X*X+1)+1)/X
Inverse Hyperbolic Secant | ARCSECH(X)=LOG(SQR(-X*X+1)+1)/X
Inverse Hyperbolic Cotangent | ARCCOTH(X)=LOG((X+1)/(X-1))/2
