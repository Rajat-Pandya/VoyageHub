∞!
pC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Services\CurrentUserService.cs
	namespace 	
	VoyageHub
 
. 
Api 
. 
Services  
;  !
public 
class 
CurrentUserService 
:  !
ICurrentUserService" 5
{		 
private

 
readonly

  
IHttpContextAccessor

 ) 
_httpContextAccessor

* >
;

> ?
public 

CurrentUserService 
(  
IHttpContextAccessor 2
httpContextAccessor3 F
)F G
{  
_httpContextAccessor 
= 
httpContextAccessor 2
;2 3
} 
public 

Guid 
? 
UserId 
{ 
get 
{ 	
var 
user 
=  
_httpContextAccessor +
.+ ,
HttpContext, 7
?7 8
.8 9
User9 =
;= >
if 
( 
user 
== 
null 
) 
return $
null% )
;) *
var 
idStr 
= 
user 
. 
	FindFirst &
(& '

ClaimTypes' 1
.1 2
NameIdentifier2 @
)@ A
?A B
.B C
ValueC H
?? 
user 
. 
	FindFirst !
(! "
$str" '
)' (
?( )
.) *
Value* /
?? 
user 
. 
	FindFirst !
(! "
$str" &
)& '
?' (
.( )
Value) .
;. /
return 
Guid 
. 
TryParse  
(  !
idStr! &
,& '
out( +
var, /
id0 2
)2 3
?4 5
id6 8
:9 :
null; ?
;? @
} 	
} 
public   

string   
?   
UserName   
=>    
_httpContextAccessor!! 
.!! 
HttpContext!! (
?!!( )
.!!) *
User!!* .
?!!. /
.!!/ 0
	FindFirst!!0 9
(!!9 :

ClaimTypes!!: D
.!!D E
Name!!E I
)!!I J
?!!J K
.!!K L
Value!!L Q
??"" 
 
_httpContextAccessor"" 
.""  
HttpContext""  +
?""+ ,
."", -
User""- 1
?""1 2
.""2 3
	FindFirst""3 <
(""< =
$str""= C
)""C D
?""D E
.""E F
Value""F K
;""K L
public$$ 

string$$ 
?$$ 
	UserEmail$$ 
=>$$  
_httpContextAccessor%% 
.%% 
HttpContext%% (
?%%( )
.%%) *
User%%* .
?%%. /
.%%/ 0
	FindFirst%%0 9
(%%9 :

ClaimTypes%%: D
.%%D E
Email%%E J
)%%J K
?%%K L
.%%L M
Value%%M R
??&& 
 
_httpContextAccessor&& 
.&&  
HttpContext&&  +
?&&+ ,
.&&, -
User&&- 1
?&&1 2
.&&2 3
	FindFirst&&3 <
(&&< =
$str&&= D
)&&D E
?&&E F
.&&F G
Value&&G L
;&&L M
public(( 

string(( 
?(( 
UserRole(( 
=>((  
_httpContextAccessor)) 
.)) 
HttpContext)) (
?))( )
.))) *
User))* .
?)). /
.))/ 0
	FindFirst))0 9
())9 :

ClaimTypes)): D
.))D E
Role))E I
)))I J
?))J K
.))K L
Value))L Q
??** 
 
_httpContextAccessor** 
.**  
HttpContext**  +
?**+ ,
.**, -
User**- 1
?**1 2
.**2 3
	FindFirst**3 <
(**< =
$str**= C
)**C D
?**D E
.**E F
Value**F K
;**K L
}++ ö†
\C:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Program.cs
var 
builder 
= 
WebApplication 
. 
CreateBuilder *
(* +
args+ /
)/ 0
;0 1
var 
connectionString 
= 
builder 
. 
Configuration ,
., -
GetConnectionString- @
(@ A
$strA T
)T U
;U V
var 
jwtKey 

= 
builder 
. 
Configuration "
[" #
$str# ,
], -
;- .
var 
	jwtIssuer 
= 
builder 
. 
Configuration %
[% &
$str& 2
]2 3
??4 6
$str7 F
;F G
var 
jwtAudience 
= 
builder 
. 
Configuration '
[' (
$str( 6
]6 7
??8 :
$str; M
;M N
if 
( 
! 
builder 
. 
Environment 
. 
IsDevelopment &
(& '
)' (
)( )
{ 
if 
( 
string 
. 
IsNullOrWhiteSpace !
(! "
connectionString" 2
)2 3
)3 4
{ 
throw 
new %
InvalidOperationException +
(+ ,
$str	, ú
)
ú ù
;
ù û
} 
if 
( 
string 
. 
IsNullOrWhiteSpace !
(! "
jwtKey" (
)( )
)) *
{ 
throw 
new %
InvalidOperationException +
(+ ,
$str, w
)w x
;x y
} 
if 
( 
jwtKey 
. 
Length 
< 
$num 
) 
{   
throw!! 
new!! %
InvalidOperationException!! +
(!!+ ,
$"!!, .
$str	!!. Ö
{
!!Ö Ü
jwtKey
!!Ü å
.
!!å ç
Length
!!ç ì
}
!!ì î
$str
!!î ñ
"
!!ñ ó
)
!!ó ò
;
!!ò ô
}"" 
}## 
else$$ 
{%% 
jwtKey'' 

??='' 
$str'' M
;''M N
connectionString(( 
??=(( 
$str	(( Ö
;
((Ö Ü
})) 
builder,, 
.,, 
Services,, 
.,, 
AddDbContext,, 
<,, 
AppDbContext,, *
>,,* +
(,,+ ,
options,,, 3
=>,,4 6
options-- 
.-- 
UseSqlServer-- 
(-- 
connectionString.. 
,.. 
sqlServerOptions// 
=>// 
sqlServerOptions// ,
.//, - 
EnableRetryOnFailure//- A
(//A B
maxRetryCount00 
:00 
$num00 
,00 
maxRetryDelay11 
:11 
TimeSpan11 #
.11# $
FromSeconds11$ /
(11/ 0
$num110 2
)112 3
,113 4
errorNumbersToAdd22 
:22 
null22 #
)33 	
)44 
)44 
;44 
builder77 
.77 
Services77 
.77 "
AddHttpContextAccessor77 '
(77' (
)77( )
;77) *
builder88 
.88 
Services88 
.88 
	AddScoped88 
<88 
ICurrentUserService88 .
,88. /
CurrentUserService880 B
>88B C
(88C D
)88D E
;88E F
builder99 
.99 
Services99 
.99 
	AddScoped99 
<99 
IJwtTokenService99 +
,99+ ,
JwtTokenService99- <
>99< =
(99= >
)99> ?
;99? @
builder<< 
.<< 
Services<< 
.<< 
	AddScoped<< 
(<< 
typeof<< !
(<<! "
IRepository<<" -
<<<- .
><<. /
)<</ 0
,<<0 1
typeof<<2 8
(<<8 9

Repository<<9 C
<<<C D
><<D E
)<<E F
)<<F G
;<<G H
builder== 
.== 
Services== 
.== 
	AddScoped== 
<== 
IAuditLogRepository== .
,==. /
AuditLogRepository==0 B
>==B C
(==C D
)==D E
;==E F
builder>> 
.>> 
Services>> 
.>> 
	AddScoped>> 
<>> 
IPolicyRepository>> ,
,>>, -
PolicyRepository>>. >
>>>> ?
(>>? @
)>>@ A
;>>A B
builder?? 
.?? 
Services?? 
.?? 
	AddScoped?? 
<?? $
ISupportTicketRepository?? 3
,??3 4#
SupportTicketRepository??5 L
>??L M
(??M N
)??N O
;??O P
builder@@ 
.@@ 
Services@@ 
.@@ 
	AddScoped@@ 
<@@ 
IUserRepository@@ *
,@@* +
UserRepository@@, :
>@@: ;
(@@; <
)@@< =
;@@= >
builderAA 
.AA 
ServicesAA 
.AA 
	AddScopedAA 
<AA 
IExpenseRepositoryAA -
,AA- .
ExpenseRepositoryAA/ @
>AA@ A
(AAA B
)AAB C
;AAC D
builderBB 
.BB 
ServicesBB 
.BB 
	AddScopedBB 
<BB #
IApprovalStepRepositoryBB 2
,BB2 3"
ApprovalStepRepositoryBB4 J
>BBJ K
(BBK L
)BBL M
;BBM N
builderCC 
.CC 
ServicesCC 
.CC 
	AddScopedCC 
<CC $
ITravelRequestRepositoryCC 3
,CC3 4#
TravelRequestRepositoryCC5 L
>CCL M
(CCM N
)CCN O
;CCO P
builderDD 
.DD 
ServicesDD 
.DD 
	AddScopedDD 
<DD 
IAuditServiceDD (
,DD( )
AuditServiceDD* 6
>DD6 7
(DD7 8
)DD8 9
;DD9 :
builderEE 
.EE 
ServicesEE 
.EE 
	AddScopedEE 
<EE 
IPolicyServiceEE )
,EE) *
PolicyServiceEE+ 8
>EE8 9
(EE9 :
)EE: ;
;EE; <
builderFF 
.FF 
ServicesFF 
.FF 
	AddScopedFF 
<FF 
ISupportServiceFF *
,FF* +
SupportServiceFF, :
>FF: ;
(FF; <
)FF< =
;FF= >
builderGG 
.GG 
ServicesGG 
.GG 
	AddScopedGG 
<GG 
IAnalyticsServiceGG ,
,GG, -
AnalyticsServiceGG. >
>GG> ?
(GG? @
)GG@ A
;GGA B
builderHH 
.HH 
ServicesHH 
.HH 
	AddScopedHH 
<HH 
IAuthServiceHH '
,HH' (
AuthServiceHH) 4
>HH4 5
(HH5 6
)HH6 7
;HH7 8
builderII 
.II 
ServicesII 
.II 
	AddScopedII 
<II 
IExpenseServiceII *
,II* +
ExpenseServiceII, :
>II: ;
(II; <
)II< =
;II= >
builderJJ 
.JJ 
ServicesJJ 
.JJ 
	AddScopedJJ 
<JJ 
IApprovalServiceJJ +
,JJ+ ,
ApprovalServiceJJ- <
>JJ< =
(JJ= >
)JJ> ?
;JJ? @
builderKK 
.KK 
ServicesKK 
.KK 
	AddScopedKK 
<KK !
ITravelRequestServiceKK 0
,KK0 1 
TravelRequestServiceKK2 F
>KKF G
(KKG H
)KKH I
;KKI J
builderLL 
.LL 
ServicesLL 
.LL 
	AddScopedLL 
<LL 
IEmployeeServiceLL +
,LL+ ,
EmployeeServiceLL- <
>LL< =
(LL= >
)LL> ?
;LL? @
builderPP 
.PP 
ServicesPP 
.PP 
AddAuthenticationPP "
(PP" #
optionsPP# *
=>PP+ -
{QQ 
optionsRR 
.RR %
DefaultAuthenticateSchemeRR %
=RR& '
JwtBearerDefaultsRR( 9
.RR9 : 
AuthenticationSchemeRR: N
;RRN O
optionsSS 
.SS "
DefaultChallengeSchemeSS "
=SS# $
JwtBearerDefaultsSS% 6
.SS6 7 
AuthenticationSchemeSS7 K
;SSK L
}TT 
)TT 
.UU 
AddJwtBearerUU 
(UU 
optionsUU 
=>UU 
{VV 
optionsWW 
.WW  
RequireHttpsMetadataWW  
=WW! "
falseWW# (
;WW( )
optionsXX 
.XX 
	SaveTokenXX 
=XX 
trueXX 
;XX 
optionsYY 
.YY %
TokenValidationParametersYY %
=YY& '
newYY( +%
TokenValidationParametersYY, E
{ZZ 
ValidateIssuer[[ 
=[[ 
true[[ 
,[[ 
ValidateAudience\\ 
=\\ 
true\\ 
,\\  
ValidateLifetime]] 
=]] 
true]] 
,]]  $
ValidateIssuerSigningKey^^  
=^^! "
true^^# '
,^^' (
ValidIssuer__ 
=__ 
	jwtIssuer__ 
,__  
ValidAudience`` 
=`` 
jwtAudience`` #
,``# $
IssuerSigningKeyaa 
=aa 
newaa  
SymmetricSecurityKeyaa 3
(aa3 4
Encodingaa4 <
.aa< =
UTF8aa= A
.aaA B
GetBytesaaB J
(aaJ K
jwtKeyaaK Q
)aaQ R
)aaR S
,aaS T
RoleClaimTypebb 
=bb 
Systembb 
.bb 
Securitybb '
.bb' (
Claimsbb( .
.bb. /

ClaimTypesbb/ 9
.bb9 :
Rolebb: >
,bb> ?
NameClaimTypecc 
=cc 
Systemcc 
.cc 
Securitycc '
.cc' (
Claimscc( .
.cc. /

ClaimTypescc/ 9
.cc9 :
Namecc: >
}dd 
;dd 
}ee 
)ee 
;ee 
buildergg 
.gg 
Servicesgg 
.gg 
AddAuthorizationgg !
(gg! "
)gg" #
;gg# $
builderjj 
.jj 
Servicesjj 
.jj 
AddControllersjj 
(jj  
)jj  !
.kk 
AddJsonOptionskk 
(kk 
optionskk 
=>kk 
{ll 
optionsmm 
.mm !
JsonSerializerOptionsmm %
.mm% & 
PropertyNamingPolicymm& :
=mm; <
Systemmm= C
.mmC D
TextmmD H
.mmH I
JsonmmI M
.mmM N
JsonNamingPolicymmN ^
.mm^ _
	CamelCasemm_ h
;mmh i
}nn 
)nn 
;nn 
varqq 
corsOriginsqq 
=qq 
builderqq 
.qq 
Configurationqq '
.qq' (

GetSectionqq( 2
(qq2 3
$strqq3 H
)qqH I
.qqI J
GetqqJ M
<qqM N
stringqqN T
[qqT U
]qqU V
>qqV W
(qqW X
)qqX Y
??qqZ \
Arrayqq] b
.qqb c
Emptyqqc h
<qqh i
stringqqi o
>qqo p
(qqp q
)qqq r
;qqr s
ifss 
(ss 
!ss 
builderss 
.ss 
Environmentss 
.ss 
IsDevelopmentss &
(ss& '
)ss' (
)ss( )
{tt 
varuu 

envOriginsuu 
=uu 
Environmentuu  
.uu  !#
GetEnvironmentVariablesuu! 8
(uu8 9
)uu9 :
.uu: ;
Keysuu; ?
.vv 	
Castvv	 
<vv 
stringvv 
>vv 
(vv 
)vv 
.ww 	
Whereww	 
(ww 
kww 
=>ww 
kww 
.ww 

StartsWithww  
(ww  !
$strww! 7
,ww7 8
StringComparisonww9 I
.wwI J
OrdinalIgnoreCasewwJ [
)ww[ \
||ww] _
kxx 
.xx 

StartsWithxx  
(xx  !
$strxx! 6
,xx6 7
StringComparisonxx8 H
.xxH I
OrdinalIgnoreCasexxI Z
)xxZ [
)xx[ \
.yy 	
Selectyy	 
(yy 
kyy 
=>yy 
Environmentyy  
.yy  !"
GetEnvironmentVariableyy! 7
(yy7 8
kyy8 9
)yy9 :
)yy: ;
.zz 	
Wherezz	 
(zz 
vzz 
=>zz 
!zz 
stringzz 
.zz 
IsNullOrWhiteSpacezz .
(zz. /
vzz/ 0
)zz0 1
)zz1 2
.{{ 	
Select{{	 
({{ 
v{{ 
=>{{ 
v{{ 
!{{ 
){{ 
.|| 	
ToArray||	 
(|| 
)|| 
;|| 
if~~ 
(~~ 

envOrigins~~ 
.~~ 
Length~~ 
>~~ 
$num~~ 
)~~ 
{ 
corsOrigins
ÄÄ 
=
ÄÄ 

envOrigins
ÄÄ  
;
ÄÄ  !
}
ÅÅ 
else
ÇÇ 
{
ÉÉ 
corsOrigins
ÑÑ 
=
ÑÑ 
corsOrigins
ÑÑ !
.
ÑÑ! "
Where
ÑÑ" '
(
ÑÑ' (
o
ÑÑ( )
=>
ÑÑ* ,
!
ÑÑ- .
o
ÑÑ. /
.
ÑÑ/ 0
Contains
ÑÑ0 8
(
ÑÑ8 9
$str
ÑÑ9 D
,
ÑÑD E
StringComparison
ÑÑF V
.
ÑÑV W
OrdinalIgnoreCase
ÑÑW h
)
ÑÑh i
&&
ÑÑj l
!
ÑÑm n
o
ÑÑn o
.
ÑÑo p
Contains
ÑÑp x
(
ÑÑx y
$strÑÑy Ñ
)ÑÑÑ Ö
)ÑÑÖ Ü
.ÑÑÜ á
ToArrayÑÑá é
(ÑÑé è
)ÑÑè ê
;ÑÑê ë
}
ÖÖ 
}ÜÜ 
builderàà 
.
àà 
Services
àà 
.
àà 
AddCors
àà 
(
àà 
options
àà  
=>
àà! #
{ââ 
options
ää 
.
ää 
	AddPolicy
ää 
(
ää 
$str
ää '
,
ää' (
policy
ää) /
=>
ää0 2
{
ãã 
if
åå 

(
åå 
corsOrigins
åå 
.
åå 
Length
åå 
>
åå  
$num
åå! "
)
åå" #
{
çç 	
policy
éé 
.
éé 
WithOrigins
éé 
(
éé 
corsOrigins
éé *
)
éé* +
;
éé+ ,
}
èè 	
policy
êê 
.
êê 
AllowAnyHeader
êê 
(
êê 
)
êê 
.
ëë 
AllowAnyMethod
ëë 
(
ëë 
)
ëë 
.
íí 
AllowCredentials
íí 
(
íí 
)
íí 
;
íí  
}
ìì 
)
ìì 
;
ìì 
}îî 
)
îî 
;
îî 
builderóó 
.
óó 
Services
óó 
.
óó %
AddEndpointsApiExplorer
óó (
(
óó( )
)
óó) *
;
óó* +
builderòò 
.
òò 
Services
òò 
.
òò 
AddSwaggerGen
òò 
(
òò 
options
òò &
=>
òò' )
{ôô 
options
öö 
.
öö 

SwaggerDoc
öö 
(
öö 
$str
öö 
,
öö 
new
öö  
OpenApiInfo
öö! ,
{
õõ 
Title
úú 
=
úú 
$str
úú 
,
úú  
Version
ùù 
=
ùù 
$str
ùù 
,
ùù 
Description
ûû 
=
ûû 
$str
ûû j
}
üü 
)
üü 
;
üü 
options
°° 
.
°° #
AddSecurityDefinition
°° !
(
°°! "
$str
°°" *
,
°°* +
new
°°, /#
OpenApiSecurityScheme
°°0 E
{
¢¢ 
Name
££ 
=
££ 
$str
££ 
,
££ 
Description
§§ 
=
§§ 
$str
§§ J
,
§§J K
In
•• 

=
•• 
ParameterLocation
•• 
.
•• 
Header
•• %
,
••% &
Type
¶¶ 
=
¶¶  
SecuritySchemeType
¶¶ !
.
¶¶! "
Http
¶¶" &
,
¶¶& '
Scheme
ßß 
=
ßß 
$str
ßß 
,
ßß 
BearerFormat
®® 
=
®® 
$str
®® 
}
©© 
)
©© 
;
©© 
options
´´ 
.
´´ $
AddSecurityRequirement
´´ "
(
´´" #
_
´´# $
=>
´´% '
new
´´( +(
OpenApiSecurityRequirement
´´, F
{
¨¨ 
{
≠≠ 	
new
ÆÆ ,
OpenApiSecuritySchemeReference
ÆÆ .
(
ÆÆ. /
$str
ÆÆ/ 7
)
ÆÆ7 8
,
ÆÆ8 9
new
ØØ 
List
ØØ 
<
ØØ 
string
ØØ 
>
ØØ 
(
ØØ 
)
ØØ 
}
∞∞ 	
}
±± 
)
±± 
;
±± 
}≤≤ 
)
≤≤ 
;
≤≤ 
var¥¥ 
app
¥¥ 
=
¥¥ 	
builder
¥¥
 
.
¥¥ 
Build
¥¥ 
(
¥¥ 
)
¥¥ 
;
¥¥ 
var∂∂ 
swaggerEnabled
∂∂ 
=
∂∂ 
builder
∂∂ 
.
∂∂ 
Configuration
∂∂ *
.
∂∂* +
GetValue
∂∂+ 3
<
∂∂3 4
bool
∂∂4 8
>
∂∂8 9
(
∂∂9 :
$str
∂∂: K
,
∂∂K L
builder
∂∂M T
.
∂∂T U
Environment
∂∂U `
.
∂∂` a
IsDevelopment
∂∂a n
(
∂∂n o
)
∂∂o p
)
∂∂p q
;
∂∂q r
if∏∏ 
(
∏∏ 
swaggerEnabled
∏∏ 
)
∏∏ 
{ππ 
app
∫∫ 
.
∫∫ 

UseSwagger
∫∫ 
(
∫∫ 
)
∫∫ 
;
∫∫ 
app
ªª 
.
ªª 
UseSwaggerUI
ªª 
(
ªª 
c
ªª 
=>
ªª 
{
ºº 
c
ΩΩ 	
.
ΩΩ	 

SwaggerEndpoint
ΩΩ
 
(
ΩΩ 
$str
ΩΩ 4
,
ΩΩ4 5
$str
ΩΩ6 H
)
ΩΩH I
;
ΩΩI J
c
ææ 	
.
ææ	 

RoutePrefix
ææ
 
=
ææ 
$str
ææ !
;
ææ! "
}
øø 
)
øø 
;
øø 
}¿¿ 
app√√ 
.
√√ 
MapGet
√√ 

(
√√
 
$str
√√ 
,
√√ 
(
√√ 
)
√√ 
=>
√√ 
swaggerEnabled
√√ $
?
ƒƒ 
Results
ƒƒ 
.
ƒƒ 
Redirect
ƒƒ 
(
ƒƒ 
$str
ƒƒ !
)
ƒƒ! "
:
≈≈ 
Results
≈≈ 
.
≈≈ 
Text
≈≈ 
(
≈≈ 
$str
≈≈ .
,
≈≈. /
$str
≈≈0 <
)
≈≈< =
)
≈≈= >
;
≈≈> ?
app«« 
.
«« 
UseCors
«« 
(
«« 
$str
«« 
)
«« 
;
«« 
app…… 
.
…… !
UseHttpsRedirection
…… 
(
…… 
)
…… 
;
…… 
appÀÀ 
.
ÀÀ 
UseAuthentication
ÀÀ 
(
ÀÀ 
)
ÀÀ 
;
ÀÀ 
appÃÃ 
.
ÃÃ 
UseAuthorization
ÃÃ 
(
ÃÃ 
)
ÃÃ 
;
ÃÃ 
appŒŒ 
.
ŒŒ 
MapControllers
ŒŒ 
(
ŒŒ 
)
ŒŒ 
;
ŒŒ 
await–– 
app
–– 	
.
––	 

RunAsync
––
 
(
–– 
)
–– 
;
–– c
aC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\GlobalUsings.cs°Y
tC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\EmployeesController.cs
	namespace 	
	VoyageHub
 
. 
Api 
. 
Controllers #
;# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

(
 
Roles 
= 
$str 
) 
] 
public 
class 
EmployeesController  
:! "
ControllerBase# 1
{ 
private 
readonly 
IEmployeeService %
_employeeService& 6
;6 7
public 

EmployeesController 
( 
IEmployeeService /
employeeService0 ?
)? @
{ 
_employeeService 
= 
employeeService *
;* +
} 
[ 
HttpGet 
] 
public 

async 
Task 
< 
ActionResult "
<" #
List# '
<' (
EmployeeDto( 3
>3 4
>4 5
>5 6
GetEmployees7 C
(C D
[ 	
	FromQuery	 
] 
string 
? 
search "
," #
[ 	
	FromQuery	 
] 
string 
? 

department &
,& '
[ 	
	FromQuery	 
] 
string 
? 
role  
,  !
CancellationToken   
cancellationToken   +
)  + ,
{!! 
var"" 
	employees"" 
="" 
await"" 
_employeeService"" .
."". /
GetEmployeesAsync""/ @
(""@ A
search""A G
,""G H

department""I S
,""S T
role""U Y
,""Y Z
cancellationToken""[ l
)""l m
;""m n
return## 
Ok## 
(## 
	employees## 
)## 
;## 
}$$ 
[&& 
HttpGet&& 
(&& 
$str&& 
)&& 
]&& 
public'' 

async'' 
Task'' 
<'' 
ActionResult'' "
<''" #
EmployeeDto''# .
>''. /
>''/ 0
GetEmployeeById''1 @
(''@ A
Guid''A E
id''F H
,''H I
CancellationToken''J [
cancellationToken''\ m
)''m n
{(( 
var)) 
employee)) 
=)) 
await)) 
_employeeService)) -
.))- . 
GetEmployeeByIdAsync)). B
())B C
id))C E
,))E F
cancellationToken))G X
)))X Y
;))Y Z
if** 

(** 
employee** 
==** 
null** 
)** 
{++ 	
return,, 
NotFound,, 
(,, 
new,, 
{,,  !
message,," )
=,,* +
$str,,, A
},,B C
),,C D
;,,D E
}-- 	
return.. 
Ok.. 
(.. 
employee.. 
).. 
;.. 
}// 
[11 
HttpPost11 
]11 
public22 

async22 
Task22 
<22 
ActionResult22 "
<22" #
EmployeeDto22# .
>22. /
>22/ 0
CreateEmployee221 ?
(22? @
[22@ A
FromBody22A I
]22I J
CreateEmployeeDto22K \
dto22] `
,22` a
CancellationToken22b s
cancellationToken	22t Ö
)
22Ö Ü
{33 
var44 
(44 
success44 
,44 
message44 
,44 
employee44 '
)44' (
=44) *
await44+ 0
_employeeService441 A
.44A B
CreateEmployeeAsync44B U
(44U V
dto44V Y
,44Y Z
cancellationToken44[ l
)44l m
;44m n
if55 

(55 
!55 
success55 
)55 
{66 	
return77 

BadRequest77 
(77 
new77 !
{77" #
message77$ +
}77, -
)77- .
;77. /
}88 	
return99 
CreatedAtAction99 
(99 
nameof99 %
(99% &
GetEmployeeById99& 5
)995 6
,996 7
new998 ;
{99< =
id99> @
=99A B
employee99C K
!99K L
.99L M
Id99M O
}99P Q
,99Q R
employee99S [
)99[ \
;99\ ]
}:: 
[<< 
HttpPut<< 
(<< 
$str<< 
)<< 
]<< 
public== 

async== 
Task== 
<== 
ActionResult== "
<==" #
EmployeeDto==# .
>==. /
>==/ 0
UpdateEmployee==1 ?
(==? @
Guid==@ D
id==E G
,==G H
[==I J
FromBody==J R
]==R S
UpdateEmployeeDto==T e
dto==f i
,==i j
CancellationToken==k |
cancellationToken	==} é
)
==é è
{>> 
var?? 
(?? 
success?? 
,?? 
message?? 
,?? 
employee?? '
)??' (
=??) *
await??+ 0
_employeeService??1 A
.??A B
UpdateEmployeeAsync??B U
(??U V
id??V X
,??X Y
dto??Z ]
,??] ^
cancellationToken??_ p
)??p q
;??q r
if@@ 

(@@ 
!@@ 
success@@ 
)@@ 
{AA 	
returnBB 

BadRequestBB 
(BB 
newBB !
{BB" #
messageBB$ +
}BB, -
)BB- .
;BB. /
}CC 	
returnDD 
OkDD 
(DD 
employeeDD 
)DD 
;DD 
}EE 
[GG 

HttpDeleteGG 
(GG 
$strGG 
)GG 
]GG 
publicHH 

asyncHH 
TaskHH 
<HH 
IActionResultHH #
>HH# $
DeleteEmployeeHH% 3
(HH3 4
GuidHH4 8
idHH9 ;
,HH; <
CancellationTokenHH= N
cancellationTokenHHO `
)HH` a
{II 
varJJ 
(JJ 
successJJ 
,JJ 
messageJJ 
)JJ 
=JJ  
awaitJJ! &
_employeeServiceJJ' 7
.JJ7 8
DeleteEmployeeAsyncJJ8 K
(JJK L
idJJL N
,JJN O
cancellationTokenJJP a
)JJa b
;JJb c
ifKK 

(KK 
!KK 
successKK 
)KK 
{LL 	
returnMM 

BadRequestMM 
(MM 
newMM !
{MM" #
messageMM$ +
}MM, -
)MM- .
;MM. /
}NN 	
returnOO 
OkOO 
(OO 
newOO 
{OO 
successOO 
=OO  !
trueOO" &
,OO& '
messageOO( /
}OO0 1
)OO1 2
;OO2 3
}PP 
[RR 
HttpPostRR 
(RR 
$strRR 
)RR 
]RR 
publicSS 

asyncSS 
TaskSS 
<SS 
ActionResultSS "
<SS" # 
BatchUploadResultDtoSS# 7
>SS7 8
>SS8 9
BatchUploadSS: E
(SSE F
	IFormFileSSF O
?SSO P
fileSSQ U
,SSU V
CancellationTokenSSW h
cancellationTokenSSi z
)SSz {
{TT 
varVV 

uploadFileVV 
=VV 
fileVV 
??VV  
(VV! "
RequestVV" )
.VV) *
HasFormContentTypeVV* <
&&VV= ?
RequestVV@ G
.VVG H
FormVVH L
.VVL M
FilesVVM R
.VVR S
CountVVS X
>VVY Z
$numVV[ \
?VV] ^
RequestVV_ f
.VVf g
FormVVg k
.VVk l
FilesVVl q
[VVq r
$numVVr s
]VVs t
:VVu v
nullVVw {
)VV{ |
;VV| }
ifXX 

(XX 

uploadFileXX 
==XX 
nullXX 
||XX !

uploadFileXX" ,
.XX, -
LengthXX- 3
==XX4 6
$numXX7 8
)XX8 9
{YY 	
returnZZ 

BadRequestZZ 
(ZZ 
newZZ !
{ZZ" #
messageZZ$ +
=ZZ, -
$strZZ. P
}ZZQ R
)ZZR S
;ZZS T
}[[ 	
if]] 

(]] 
!]] 

uploadFile]] 
.]] 
FileName]]  
.]]  !
EndsWith]]! )
(]]) *
$str]]* 0
,]]0 1
StringComparison]]2 B
.]]B C
OrdinalIgnoreCase]]C T
)]]T U
)]]U V
{^^ 	
return__ 

BadRequest__ 
(__ 
new__ !
{__" #
message__$ +
=__, -
$str__. _
}__` a
)__a b
;__b c
}`` 	
usingbb 
varbb 
streambb 
=bb 

uploadFilebb %
.bb% &
OpenReadStreambb& 4
(bb4 5
)bb5 6
;bb6 7
varcc 
resultcc 
=cc 
awaitcc 
_employeeServicecc +
.cc+ ,
BatchUploadCsvAsynccc, ?
(cc? @
streamcc@ F
,ccF G
cancellationTokenccH Y
)ccY Z
;ccZ [
returndd 
Okdd 
(dd 
resultdd 
)dd 
;dd 
}ee 
[gg 
HttpGetgg 
(gg 
$strgg 
)gg 
]gg 
[hh 
AllowAnonymoushh 
]hh 
publicii 

IActionResultii 
DownloadCsvTemplateii ,
(ii, -
)ii- .
{jj 
varkk 

csvBuilderkk 
=kk 
newkk 
StringBuilderkk *
(kk* +
)kk+ ,
;kk, -

csvBuilderll 
.ll 

AppendLinell 
(ll 
$strll C
)llC D
;llD E

csvBuildermm 
.mm 

AppendLinemm 
(mm 
$strmm h
)mmh i
;mmi j

csvBuildernn 
.nn 

AppendLinenn 
(nn 
$strnn l
)nnl m
;nnm n

csvBuilderoo 
.oo 

AppendLineoo 
(oo 
$stroo p
)oop q
;ooq r
varqq 
bytesqq 
=qq 
Encodingqq 
.qq 
UTF8qq !
.qq! "
GetBytesqq" *
(qq* +

csvBuilderqq+ 5
.qq5 6
ToStringqq6 >
(qq> ?
)qq? @
)qq@ A
;qqA B
returnrr 
Filerr 
(rr 
bytesrr 
,rr 
$strrr %
,rr% &
$strrr' ?
)rr? @
;rr@ A
}ss 
}tt ±L
rC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\SupportController.cs
	namespace 	
	VoyageHub
 
. 
Api 
. 
Controllers #
;# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

]
 
public 
class 
SupportController 
:  
ControllerBase! /
{ 
private 
readonly 
ISupportService $
_supportService% 4
;4 5
private 
readonly 
ILogger 
< 
SupportController .
>. /
_logger0 7
;7 8
public 

SupportController 
( 
ISupportService ,
supportService- ;
,; <
ILogger= D
<D E
SupportControllerE V
>V W
loggerX ^
)^ _
{ 
_supportService 
= 
supportService (
;( )
_logger 
= 
logger 
; 
} 
[ 
HttpGet 
( 
$str 
) 
] 
[ 
	Authorize 
( 
Roles 
= 
$str 7
)7 8
]8 9
public 

async 
Task 
< 
ActionResult "
<" #
List# '
<' (
SupportTicketDto( 8
>8 9
>9 :
>: ;

GetTickets< F
(F G
[ 	
	FromQuery	 
] 
string 
? 
status "
," #
[ 	
	FromQuery	 
] 
Guid 
? 
userId  
,  !
[   	
	FromQuery  	 
]   
string   
?   
search   "
,  " #
CancellationToken!! 
cancellationToken!! +
)!!+ ,
{"" 
try## 
{$$ 	
var%% 
tickets%% 
=%% 
await%% 
_supportService%%  /
.%%/ 0
GetTicketsAsync%%0 ?
(%%? @
status%%@ F
,%%F G
userId%%H N
,%%N O
search%%P V
,%%V W
cancellationToken%%X i
)%%i j
;%%j k
return&& 
Ok&& 
(&& 
tickets&& 
)&& 
;&& 
}'' 	
catch(( 
((( 
	Exception(( 
ex(( 
)(( 
{)) 	
_logger** 
.** 
LogError** 
(** 
ex** 
,**  
$str**! A
)**A B
;**B C
return++ 

StatusCode++ 
(++ 
$num++ !
,++! "
new++# &
{++' (
message++) 0
=++1 2
$"++3 5
$str++5 E
{++E F
ex++F H
.++H I
Message++I P
}++P Q
"++Q R
}++S T
)++T U
;++U V
},, 	
}-- 
[// 
HttpGet// 
(// 
$str// 
)// 
]// 
[00 
	Authorize00 
(00 
Roles00 
=00 
$str00 7
)007 8
]008 9
public11 

async11 
Task11 
<11 
ActionResult11 "
<11" #
SupportTicketDto11# 3
>113 4
>114 5
GetTicketById116 C
(11C D
Guid11D H
id11I K
,11K L
CancellationToken11M ^
cancellationToken11_ p
)11p q
{22 
try33 
{44 	
var55 
ticket55 
=55 
await55 
_supportService55 .
.55. /
GetTicketByIdAsync55/ A
(55A B
id55B D
,55D E
cancellationToken55F W
)55W X
;55X Y
if66 
(66 
ticket66 
==66 
null66 
)66 
{77 
return88 
NotFound88 
(88  
new88  #
{88$ %
message88& -
=88. /
$str880 K
}88L M
)88M N
;88N O
}99 
return;; 
Ok;; 
(;; 
ticket;; 
);; 
;;; 
}<< 	
catch== 
(== 
	Exception== 
ex== 
)== 
{>> 	
_logger?? 
.?? 
LogError?? 
(?? 
ex?? 
,??  
$str??! @
)??@ A
;??A B
return@@ 

StatusCode@@ 
(@@ 
$num@@ !
,@@! "
new@@# &
{@@' (
message@@) 0
=@@1 2
$"@@3 5
$str@@5 E
{@@E F
ex@@F H
.@@H I
Message@@I P
}@@P Q
"@@Q R
}@@S T
)@@T U
;@@U V
}AA 	
}BB 
[DD 
HttpPostDD 
(DD 
$strDD 
)DD 
]DD 
[EE 
	AuthorizeEE 
(EE 
RolesEE 
=EE 
$strEE 7
)EE7 8
]EE8 9
publicFF 

asyncFF 
TaskFF 
<FF 
ActionResultFF "
<FF" #
SupportTicketDtoFF# 3
>FF3 4
>FF4 5
CreateTicketFF6 B
(FFB C
[FFC D
FromBodyFFD L
]FFL M"
CreateSupportTicketDtoFFN d
dtoFFe h
,FFh i
CancellationTokenFFj {
cancellationToken	FF| ç
)
FFç é
{GG 
tryHH 
{II 	
varJJ 
(JJ 
successJJ 
,JJ 
messageJJ !
,JJ! "
ticketJJ# )
)JJ) *
=JJ+ ,
awaitJJ- 2
_supportServiceJJ3 B
.JJB C
CreateTicketAsyncJJC T
(JJT U
dtoJJU X
,JJX Y
cancellationTokenJJZ k
)JJk l
;JJl m
ifKK 
(KK 
!KK 
successKK 
)KK 
{LL 
returnMM 

BadRequestMM !
(MM! "
newMM" %
{MM& '
messageMM( /
}MM0 1
)MM1 2
;MM2 3
}NN 
returnPP 
CreatedAtActionPP "
(PP" #
nameofPP# )
(PP) *
GetTicketByIdPP* 7
)PP7 8
,PP8 9
newPP: =
{PP> ?
idPP@ B
=PPC D
ticketPPE K
!PPK L
.PPL M
IdPPM O
}PPP Q
,PPQ R
ticketPPS Y
)PPY Z
;PPZ [
}QQ 	
catchRR 
(RR 
	ExceptionRR 
exRR 
)RR 
{SS 	
_loggerTT 
.TT 
LogErrorTT 
(TT 
exTT 
,TT  
$strTT! @
)TT@ A
;TTA B
returnUU 

StatusCodeUU 
(UU 
$numUU !
,UU! "
newUU# &
{UU' (
messageUU) 0
=UU1 2
$"UU3 5
$strUU5 E
{UUE F
exUUF H
.UUH I
MessageUUI P
}UUP Q
"UUQ R
}UUS T
)UUT U
;UUU V
}VV 	
}WW 
[YY 
HttpPostYY 
(YY 
$strYY #
)YY# $
]YY$ %
[ZZ 
	AuthorizeZZ 
(ZZ 
RolesZZ 
=ZZ 
$strZZ .
)ZZ. /
]ZZ/ 0
public[[ 

async[[ 
Task[[ 
<[[ 
IActionResult[[ #
>[[# $
UpdateTicketStatus[[% 7
([[7 8
Guid[[8 <
id[[= ?
,[[? @
[[[A B
FromBody[[B J
][[J K!
UpdateTicketStatusDto[[L a
dto[[b e
,[[e f
CancellationToken[[g x
cancellationToken	[[y ä
)
[[ä ã
{\\ 
try]] 
{^^ 	
var__ 
(__ 
success__ 
,__ 
message__ !
)__! "
=__# $
await__% *
_supportService__+ :
.__: ;#
UpdateTicketStatusAsync__; R
(__R S
id__S U
,__U V
dto__W Z
,__Z [
cancellationToken__\ m
)__m n
;__n o
if`` 
(`` 
!`` 
success`` 
)`` 
{aa 
returnbb 
NotFoundbb 
(bb  
newbb  #
{bb$ %
messagebb& -
}bb. /
)bb/ 0
;bb0 1
}cc 
returnee 
Okee 
(ee 
newee 
{ee 
successee #
=ee$ %
trueee& *
,ee* +
messageee, 3
}ee4 5
)ee5 6
;ee6 7
}ff 	
catchgg 
(gg 
	Exceptiongg 
exgg 
)gg 
{hh 	
_loggerii 
.ii 
LogErrorii 
(ii 
exii 
,ii  
$strii! G
)iiG H
;iiH I
returnjj 

StatusCodejj 
(jj 
$numjj !
,jj! "
newjj# &
{jj' (
messagejj) 0
=jj1 2
$"jj3 5
$strjj5 E
{jjE F
exjjF H
.jjH I
MessagejjI P
}jjP Q
"jjQ R
}jjS T
)jjT U
;jjU V
}kk 	
}ll 
}mm ı|
yC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\TravelRequestsController.cs
	namespace

 	
	VoyageHub


 
.

 
Api

 
.

 
Controllers

 #
;

# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

]
 
public 
class $
TravelRequestsController %
:& '
ControllerBase( 6
{ 
private 
readonly !
ITravelRequestService *!
_travelRequestService+ @
;@ A
private 
readonly 
ICurrentUserService (
_currentUserService) <
;< =
private 
readonly 
ILogger 
< $
TravelRequestsController 5
>5 6
_logger7 >
;> ?
public 
$
TravelRequestsController #
(# $!
ITravelRequestService  
travelRequestService 2
,2 3
ICurrentUserService 
currentUserService .
,. /
ILogger 
< $
TravelRequestsController (
>( )
logger* 0
)0 1
{ !
_travelRequestService 
=  
travelRequestService  4
;4 5
_currentUserService 
= 
currentUserService 0
;0 1
_logger 
= 
logger 
; 
} 
[ 
HttpGet 
] 
[   
	Authorize   
(   
Roles   
=   
$str   7
)  7 8
]  8 9
public!! 

async!! 
Task!! 
<!! 
ActionResult!! "
<!!" #
List!!# '
<!!' ( 
TravelRequestListDto!!( <
>!!< =
>!!= >
>!!> ?
GetRequests!!@ K
(!!K L
["" 	
	FromQuery""	 
]"" 
string"" 
?"" 
status"" "
,""" #
[## 	
	FromQuery##	 
]## 
string## 
?## 
search## "
,##" #
[$$ 	
	FromQuery$$	 
]$$ 
Guid$$ 
?$$ 

employeeId$$ $
,$$$ %
[%% 	
	FromQuery%%	 
]%% 
int%% 
?%% 
limit%% 
)%% 
{&& 
if'' 

('' 
User'' 
.'' 
IsInRole'' 
('' 
$str'' $
)''$ %
)''% &
{(( 	
var)) 
callerId)) 
=)) 
_currentUserService)) .
.)). /
UserId))/ 5
;))5 6
if** 
(** 

employeeId** 
.** 
HasValue** #
&&**$ &
callerId**' /
.**/ 0
HasValue**0 8
&&**9 ;

employeeId**< F
.**F G
Value**G L
!=**M O
callerId**P X
.**X Y
Value**Y ^
)**^ _
{++ 
return,, 
Forbid,, 
(,, 
),, 
;,,  
}-- 
}.. 	
var00 
dtoList00 
=00 
await00 !
_travelRequestService00 1
.001 2
GetRequestsAsync002 B
(00B C
status00C I
,00I J
search00K Q
,00Q R

employeeId00S ]
,00] ^
limit00_ d
)00d e
;00e f
return11 
Ok11 
(11 
dtoList11 
)11 
;11 
}22 
[44 
HttpGet44 
(44 
$str44 
)44 
]44 
[55 
	Authorize55 
(55 
Roles55 
=55 
$str55 7
)557 8
]558 9
public66 

async66 
Task66 
<66 
ActionResult66 "
<66" #
DashboardStatsDto66# 4
>664 5
>665 6
GetStats667 ?
(66? @
[66@ A
	FromQuery66A J
]66J K
Guid66L P
?66P Q

employeeId66R \
)66\ ]
{77 
if88 

(88 
User88 
.88 
IsInRole88 
(88 
$str88 $
)88$ %
)88% &
{99 	
var:: 
callerId:: 
=:: 
_currentUserService:: .
.::. /
UserId::/ 5
;::5 6
if;; 
(;; 

employeeId;; 
.;; 
HasValue;; #
&&;;$ &
callerId;;' /
.;;/ 0
HasValue;;0 8
&&;;9 ;

employeeId;;< F
.;;F G
Value;;G L
!=;;M O
callerId;;P X
.;;X Y
Value;;Y ^
);;^ _
{<< 
return== 
Forbid== 
(== 
)== 
;==  
}>> 
}?? 	
varAA 
statsAA 
=AA 
awaitAA !
_travelRequestServiceAA /
.AA/ 0
GetStatsAsyncAA0 =
(AA= >

employeeIdAA> H
)AAH I
;AAI J
returnBB 
OkBB 
(BB 
statsBB 
)BB 
;BB 
}CC 
[EE 
HttpGetEE 
(EE 
$strEE 
)EE 
]EE 
[FF 
	AuthorizeFF 
(FF 
RolesFF 
=FF 
$strFF 7
)FF7 8
]FF8 9
publicGG 

asyncGG 
TaskGG 
<GG 
ActionResultGG "
<GG" #"
TravelRequestDetailDtoGG# 9
>GG9 :
>GG: ;
	GetDetailGG< E
(GGE F
GuidGGF J
idGGK M
)GGM N
{HH 
varII 
detailII 
=II 
awaitII !
_travelRequestServiceII 0
.II0 1
GetDetailAsyncII1 ?
(II? @
idII@ B
)IIB C
;IIC D
ifJJ 

(JJ 
detailJJ 
==JJ 
nullJJ 
)JJ 
returnJJ "
NotFoundJJ# +
(JJ+ ,
newJJ, /
{JJ0 1
messageJJ2 9
=JJ: ;
$strJJ< W
}JJX Y
)JJY Z
;JJZ [
ifLL 

(LL 
UserLL 
.LL 
IsInRoleLL 
(LL 
$strLL $
)LL$ %
)LL% &
{MM 	
varNN 
callerIdNN 
=NN 
_currentUserServiceNN .
.NN. /
UserIdNN/ 5
;NN5 6
ifOO 
(OO 
callerIdOO 
.OO 
HasValueOO !
&&OO" $
detailOO% +
.OO+ ,

EmployeeIdOO, 6
!=OO7 9
callerIdOO: B
.OOB C
ValueOOC H
)OOH I
{PP 
returnQQ 
ForbidQQ 
(QQ 
)QQ 
;QQ  
}RR 
}SS 	
returnUU 
OkUU 
(UU 
detailUU 
)UU 
;UU 
}VV 
[XX 
HttpPostXX 
]XX 
[YY 
	AuthorizeYY 
(YY 
RolesYY 
=YY 
$strYY /
)YY/ 0
]YY0 1
publicZZ 

asyncZZ 
TaskZZ 
<ZZ 
ActionResultZZ "
<ZZ" #"
TravelRequestDetailDtoZZ# 9
>ZZ9 :
>ZZ: ;
CreateZZ< B
(ZZB C
[ZZC D
FromBodyZZD L
]ZZL M"
CreateTravelRequestDtoZZN d
dtoZZe h
,ZZh i
[ZZj k
	FromQueryZZk t
]ZZt u
GuidZZv z
?ZZz {

employeeId	ZZ| Ü
)
ZZÜ á
{[[ 
if\\ 

(\\ 
dto\\ 
==\\ 
null\\ 
)\\ 
return\\ 

BadRequest\\  *
(\\* +
new\\+ .
{\\/ 0
message\\1 8
=\\9 :
$str\\; R
}\\S T
)\\T U
;\\U V
if^^ 

(^^ 
User^^ 
.^^ 
IsInRole^^ 
(^^ 
$str^^ $
)^^$ %
)^^% &
{__ 	

employeeId`` 
=`` 
_currentUserService`` ,
.``, -
UserId``- 3
;``3 4
}aa 	
varcc 
resultcc 
=cc 
awaitcc !
_travelRequestServicecc 0
.cc0 1
CreateRequestAsynccc1 C
(ccC D
dtoccD G
,ccG H

employeeIdccI S
)ccS T
;ccT U
ifdd 

(dd 
!dd 
resultdd 
.dd 
Successdd 
)dd 
returndd #

BadRequestdd$ .
(dd. /
newdd/ 2
{dd3 4
messagedd5 <
=dd= >
resultdd? E
.ddE F
MessageddF M
}ddN O
)ddO P
;ddP Q
returnff 
CreatedAtActionff 
(ff 
nameofff %
(ff% &
	GetDetailff& /
)ff/ 0
,ff0 1
newff2 5
{ff6 7
idff8 :
=ff; <
resultff= C
.ffC D
RequestffD K
!ffK L
.ffL M
IdffM O
}ffP Q
,ffQ R
resultffS Y
.ffY Z
RequestffZ a
)ffa b
;ffb c
}gg 
[ii 

HttpDeleteii 
(ii 
$strii 
)ii 
]ii 
[jj 
	Authorizejj 
(jj 
Rolesjj 
=jj 
$strjj /
)jj/ 0
]jj0 1
publickk 

asynckk 
Taskkk 
<kk 
IActionResultkk #
>kk# $
CancelRequestkk% 2
(kk2 3
Guidkk3 7
idkk8 :
)kk: ;
{ll 
varmm 
resultmm 
=mm 
awaitmm !
_travelRequestServicemm 0
.mm0 1
CancelRequestAsyncmm1 C
(mmC D
idmmD F
)mmF G
;mmG H
ifnn 

(nn 
!nn 
resultnn 
.nn 
Successnn 
)nn 
returnnn #
NotFoundnn$ ,
(nn, -
newnn- 0
{nn1 2
messagenn3 :
=nn; <
resultnn= C
.nnC D
MessagennD K
}nnL M
)nnM N
;nnN O
returnoo 
Okoo 
(oo 
newoo 
{oo 
messageoo 
=oo  !
resultoo" (
.oo( )
Messageoo) 0
}oo1 2
)oo2 3
;oo3 4
}pp 
[rr 
HttpPostrr 
(rr 
$strrr 
)rr 
]rr 
[ss 
	Authorizess 
(ss 
Rolesss 
=ss 
$strss /
)ss/ 0
]ss0 1
publictt 

asynctt 
Tasktt 
<tt 
IActionResulttt #
>tt# $
SubmitDrafttt% 0
(tt0 1
Guidtt1 5
idtt6 8
)tt8 9
{uu 
varvv 
resultvv 
=vv 
awaitvv !
_travelRequestServicevv 0
.vv0 1
SubmitDraftAsyncvv1 A
(vvA B
idvvB D
)vvD E
;vvE F
returnww 

StatusCodeww 
(ww 
resultww  
.ww  !

StatusCodeww! +
,ww+ ,
resultww- 3
.ww3 4
Responseww4 <
)ww< =
;ww= >
}xx 
[zz 
HttpPostzz 
(zz 
$strzz 
)zz 
]zz 
[{{ 
	Authorize{{ 
({{ 
Roles{{ 
={{ 
$str{{ /
){{/ 0
]{{0 1
public|| 

async|| 
Task|| 
<|| 
IActionResult|| #
>||# $

AddSegment||% /
(||/ 0
Guid||0 4
id||5 7
,||7 8
[||9 :
FromBody||: B
]||B C
AddSegmentDto||D Q
dto||R U
)||U V
{}} 
var~~ 
result~~ 
=~~ 
await~~ !
_travelRequestService~~ 0
.~~0 1
AddSegmentAsync~~1 @
(~~@ A
id~~A C
,~~C D
dto~~E H
)~~H I
;~~I J
if 

( 
! 
result 
. 
Success 
) 
return #
NotFound$ ,
(, -
new- 0
{1 2
message3 :
=; <
result= C
.C D
MessageD K
}L M
)M N
;N O
return
ÄÄ 
Ok
ÄÄ 
(
ÄÄ 
result
ÄÄ 
.
ÄÄ 
Segment
ÄÄ  
)
ÄÄ  !
;
ÄÄ! "
}
ÅÅ 
[
ÉÉ 
HttpPost
ÉÉ 
(
ÉÉ 
$str
ÉÉ 
)
ÉÉ 
]
ÉÉ 
[
ÑÑ 
	Authorize
ÑÑ 
(
ÑÑ 
Roles
ÑÑ 
=
ÑÑ 
$str
ÑÑ /
)
ÑÑ/ 0
]
ÑÑ0 1
public
ÖÖ 

async
ÖÖ 
Task
ÖÖ 
<
ÖÖ 
IActionResult
ÖÖ #
>
ÖÖ# $
AddStay
ÖÖ% ,
(
ÖÖ, -
Guid
ÖÖ- 1
id
ÖÖ2 4
,
ÖÖ4 5
[
ÖÖ6 7
FromBody
ÖÖ7 ?
]
ÖÖ? @

AddStayDto
ÖÖA K
dto
ÖÖL O
)
ÖÖO P
{
ÜÜ 
var
áá 
result
áá 
=
áá 
await
áá #
_travelRequestService
áá 0
.
áá0 1
AddStayAsync
áá1 =
(
áá= >
id
áá> @
,
áá@ A
dto
ááB E
)
ááE F
;
ááF G
if
àà 

(
àà 
!
àà 
result
àà 
.
àà 
Success
àà 
)
àà 
return
àà #
NotFound
àà$ ,
(
àà, -
new
àà- 0
{
àà1 2
message
àà3 :
=
àà; <
result
àà= C
.
ààC D
Message
ààD K
}
ààL M
)
ààM N
;
ààN O
return
ââ 
Ok
ââ 
(
ââ 
result
ââ 
.
ââ 
Stay
ââ 
)
ââ 
;
ââ 
}
ää 
[
åå 
HttpPost
åå 
(
åå 
$str
åå 
)
åå 
]
åå 
[
çç 
	Authorize
çç 
(
çç 
Roles
çç 
=
çç 
$str
çç 7
)
çç7 8
]
çç8 9
public
éé 

async
éé 
Task
éé 
<
éé 
IActionResult
éé #
>
éé# $
CloseRequest
éé% 1
(
éé1 2
Guid
éé2 6
id
éé7 9
)
éé9 :
{
èè 
var
êê 
result
êê 
=
êê 
await
êê #
_travelRequestService
êê 0
.
êê0 1
CloseRequestAsync
êê1 B
(
êêB C
id
êêC E
)
êêE F
;
êêF G
if
ëë 

(
ëë 
!
ëë 
result
ëë 
.
ëë 
Success
ëë 
)
ëë 
return
ëë #
NotFound
ëë$ ,
(
ëë, -
new
ëë- 0
{
ëë1 2
message
ëë3 :
=
ëë; <
result
ëë= C
.
ëëC D
Message
ëëD K
}
ëëL M
)
ëëM N
;
ëëN O
return
íí 
Ok
íí 
(
íí 
new
íí 
{
íí 
success
íí 
=
íí  !
true
íí" &
,
íí& '
message
íí( /
=
íí0 1
result
íí2 8
.
íí8 9
Message
íí9 @
}
ííA B
)
ííB C
;
ííC D
}
ìì 
}îî ç≥
sC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\ExpensesController.cs
	namespace 	
	VoyageHub
 
. 
Api 
. 
Controllers #
;# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

]
 
public 
class 
ExpensesController 
:  !
ControllerBase" 0
{ 
private 
static 
readonly 
HashSet #
<# $
string$ *
>* +
AllowedExtensions, =
=> ?
new@ C
(C D
StringComparerD R
.R S
OrdinalIgnoreCaseS d
)d e
{ 
$str 
, 
$str 
, 
$str 
, 
$str  '
,' (
$str) 0
} 
; 
private 
readonly 
IExpenseService $
_expenseService% 4
;4 5
private 
readonly 
IWebHostEnvironment (
_environment) 5
;5 6
public 

ExpensesController 
( 
IExpenseService -
expenseService. <
,< =
IWebHostEnvironment> Q
environmentR ]
)] ^
{ 
_expenseService 
= 
expenseService (
;( )
_environment   
=   
environment   "
;  " #
}!! 
[## 
HttpGet## 
(## 
$str## 
)## 
]## 
[$$ 
	Authorize$$ 
($$ 
Roles$$ 
=$$ 
$str$$ &
)$$& '
]$$' (
public%% 

async%% 
Task%% 
<%% 
ActionResult%% "
<%%" ##
ExpenseReviewSummaryDto%%# :
>%%: ;
>%%; <

GetPending%%= G
(%%G H
[&& 	
	FromQuery&&	 
]&& 
string&& 
?&& 
category&& $
,&&$ %
['' 	
	FromQuery''	 
]'' 
bool'' 
?'' 
highPriorityOnly'' *
,''* +
CancellationToken(( 
cancellationToken(( +
)((+ ,
{)) 
var** 
result** 
=** 
await** 
_expenseService** *
.*** +"
GetPendingSummaryAsync**+ A
(**A B
category**B J
,**J K
highPriorityOnly**L \
,**\ ]
cancellationToken**^ o
)**o p
;**p q
return++ 
Ok++ 
(++ 
result++ 
)++ 
;++ 
},, 
[.. 
HttpPost.. 
].. 
[// 
	Authorize// 
(// 
Roles// 
=// 
$str// 7
)//7 8
]//8 9
public00 

async00 
Task00 
<00 
IActionResult00 #
>00# $
SubmitExpense00% 2
(002 3
[003 4
FromBody004 <
]00< =
SubmitExpenseDto00> N
dto00O R
,00R S
CancellationToken00T e
cancellationToken00f w
)00w x
{11 
var22 
(22 
success22 
,22 
message22 
,22 
expense22 &
)22& '
=22( )
await22* /
_expenseService220 ?
.22? @
SubmitExpenseAsync22@ R
(22R S
dto22S V
,22V W
cancellationToken22X i
)22i j
;22j k
if33 

(33 
!33 
success33 
)33 
{44 	
return55 

BadRequest55 
(55 
new55 !
{55" #
message55$ +
}55, -
)55- .
;55. /
}66 	
return88 
Ok88 
(88 
expense88 
)88 
;88 
}99 
[;; 
HttpPost;; 
(;; 
$str;; 
);; 
];; 
[<< 
	Authorize<< 
(<< 
Roles<< 
=<< 
$str<< &
)<<& '
]<<' (
public== 

async== 
Task== 
<== 
IActionResult== #
>==# $

TakeAction==% /
(==/ 0
Guid==0 4
id==5 7
,==7 8
[==9 :
FromBody==: B
]==B C
ExpenseActionDto==D T
dto==U X
,==X Y
CancellationToken==Z k
cancellationToken==l }
)==} ~
{>> 
var?? 
(?? 
success?? 
,?? 
message?? 
)?? 
=??  
await??! &
_expenseService??' 6
.??6 7
TakeActionAsync??7 F
(??F G
id??G I
,??I J
dto??K N
,??N O
cancellationToken??P a
)??a b
;??b c
if@@ 

(@@ 
!@@ 
success@@ 
)@@ 
{AA 	
returnBB 
NotFoundBB 
(BB 
newBB 
{BB  !
messageBB" )
}BB* +
)BB+ ,
;BB, -
}CC 	
returnEE 
OkEE 
(EE 
newEE 
{EE 
successEE 
=EE  !
trueEE" &
,EE& '
messageEE( /
}EE0 1
)EE1 2
;EE2 3
}FF 
[HH 
HttpPostHH 
(HH 
$strHH 
)HH 
]HH 
[II 
	AuthorizeII 
(II 
RolesII 
=II 
$strII &
)II& '
]II' (
publicJJ 

asyncJJ 
TaskJJ 
<JJ 
IActionResultJJ #
>JJ# $
BatchPayoutJJ% 0
(JJ0 1
[JJ1 2
FromBodyJJ2 :
]JJ: ;
BatchPayoutDtoJJ< J
?JJJ K
dtoJJL O
,JJO P
CancellationTokenJJQ b
cancellationTokenJJc t
)JJt u
{KK 
varLL 
(LL 
successLL 
,LL 
messageLL 
)LL 
=LL  
awaitLL! &
_expenseServiceLL' 6
.LL6 7
BatchPayoutAsyncLL7 G
(LLG H
dtoLLH K
,LLK L
cancellationTokenLLM ^
)LL^ _
;LL_ `
ifMM 

(MM 
!MM 
successMM 
)MM 
{NN 	
returnOO 

BadRequestOO 
(OO 
newOO !
{OO" #
messageOO$ +
}OO, -
)OO- .
;OO. /
}PP 	
returnRR 
OkRR 
(RR 
newRR 
{RR 
successRR 
=RR  !
trueRR" &
,RR& '
messageRR( /
}RR0 1
)RR1 2
;RR2 3
}SS 
[UU 
HttpPostUU 
(UU 
$strUU 
)UU 
]UU 
[VV 
	AuthorizeVV 
(VV 
RolesVV 
=VV 
$strVV 7
)VV7 8
]VV8 9
publicWW 

asyncWW 
TaskWW 
<WW 
IActionResultWW #
>WW# $
UploadProofWW% 0
(WW0 1
	IFormFileWW1 :
?WW: ;
fileWW< @
,WW@ A
CancellationTokenWWB S
cancellationTokenWWT e
)WWe f
{XX 
varYY 

uploadFileYY 
=YY 
fileYY 
??YY  
(YY! "
RequestYY" )
.YY) *
HasFormContentTypeYY* <
&&YY= ?
RequestYY@ G
.YYG H
FormYYH L
.YYL M
FilesYYM R
.YYR S
CountYYS X
>YYY Z
$numYY[ \
?YY] ^
RequestYY_ f
.YYf g
FormYYg k
.YYk l
FilesYYl q
[YYq r
$numYYr s
]YYs t
:YYu v
nullYYw {
)YY{ |
;YY| }
if[[ 

([[ 

uploadFile[[ 
==[[ 
null[[ 
||[[ !

uploadFile[[" ,
.[[, -
Length[[- 3
==[[4 6
$num[[7 8
)[[8 9
{\\ 	
return]] 

BadRequest]] 
(]] 
new]] !
{]]" #
message]]$ +
=]], -
$str]]. Q
}]]R S
)]]S T
;]]T U
}^^ 	
if`` 

(`` 

uploadFile`` 
.`` 
Length`` 
>`` 
$num``  "
*``# $
$num``% )
*``* +
$num``, 0
)``0 1
{aa 	
returnbb 

BadRequestbb 
(bb 
newbb !
{bb" #
messagebb$ +
=bb, -
$strbb. Q
}bbR S
)bbS T
;bbT U
}cc 	
varee 
extee 
=ee 
Pathee 
.ee 
GetExtensionee #
(ee# $

uploadFileee$ .
.ee. /
FileNameee/ 7
)ee7 8
.ee8 9
ToLowerInvariantee9 I
(eeI J
)eeJ K
;eeK L
ifff 

(ff 
!ff 
AllowedExtensionsff 
.ff 
Containsff '
(ff' (
extff( +
)ff+ ,
)ff, -
{gg 	
returnhh 

BadRequesthh 
(hh 
newhh !
{hh" #
messagehh$ +
=hh, -
$strhh. 
}
hhÄ Å
)
hhÅ Ç
;
hhÇ É
}ii 	
varkk 
receiptsDirkk 
=kk 
Pathkk 
.kk 
Combinekk &
(kk& '
_environmentkk' 3
.kk3 4
ContentRootPathkk4 C
,kkC D
$strkkE O
,kkO P
$strkkQ [
)kk[ \
;kk\ ]
ifll 

(ll 
!ll 
	Directoryll 
.ll 
Existsll 
(ll 
receiptsDirll )
)ll) *
)ll* +
{mm 	
	Directorynn 
.nn 
CreateDirectorynn %
(nn% &
receiptsDirnn& 1
)nn1 2
;nn2 3
}oo 	
varqq 
safeFileNameqq 
=qq 
$"qq 
{qq 
Guidqq "
.qq" #
NewGuidqq# *
(qq* +
)qq+ ,
:qq, -
$strqq- .
}qq. /
$strqq/ 0
{qq0 1
Pathqq1 5
.qq5 6'
GetFileNameWithoutExtensionqq6 Q
(qqQ R

uploadFileqqR \
.qq\ ]
FileNameqq] e
)qqe f
.qqf g
Replaceqqg n
(qqn o
$strqqo r
,qqr s
$strqqt w
)qqw x
}qqx y
{qqy z
extqqz }
}qq} ~
"qq~ 
;	qq Ä
varrr 
filePathrr 
=rr 
Pathrr 
.rr 
Combinerr #
(rr# $
receiptsDirrr$ /
,rr/ 0
safeFileNamerr1 =
)rr= >
;rr> ?
awaittt 
usingtt 
(tt 
vartt 
streamtt 
=tt  !
newtt" %

FileStreamtt& 0
(tt0 1
filePathtt1 9
,tt9 :
FileModett; C
.ttC D
CreatettD J
)ttJ K
)ttK L
{uu 	
awaitvv 

uploadFilevv 
.vv 
CopyToAsyncvv (
(vv( )
streamvv) /
,vv/ 0
cancellationTokenvv1 B
)vvB C
;vvC D
}ww 	
varyy 
relativeUrlyy 
=yy 
$"yy 
$stryy 0
{yy0 1
safeFileNameyy1 =
}yy= >
"yy> ?
;yy? @
returnzz 
Okzz 
(zz 
newzz 
{{{ 	
success|| 
=|| 
true|| 
,|| 
	proofPath}} 
=}} 
relativeUrl}} #
,}}# $
fileName~~ 
=~~ 

uploadFile~~ !
.~~! "
FileName~~" *
,~~* +
fileUrl 
= 
$" 
{ 
Request  
.  !
Scheme! '
}' (
$str( +
{+ ,
Request, 3
.3 4
Host4 8
}8 9
{9 :
relativeUrl: E
}E F
"F G
}
ÄÄ 	
)
ÄÄ	 

;
ÄÄ
 
}
ÅÅ 
[
ÉÉ 
HttpPost
ÉÉ 
(
ÉÉ 
$str
ÉÉ 
)
ÉÉ 
]
ÉÉ 
[
ÑÑ 
	Authorize
ÑÑ 
(
ÑÑ 
Roles
ÑÑ 
=
ÑÑ 
$str
ÑÑ 7
)
ÑÑ7 8
]
ÑÑ8 9
public
ÖÖ 

async
ÖÖ 
Task
ÖÖ 
<
ÖÖ 
IActionResult
ÖÖ #
>
ÖÖ# $
AttachProof
ÖÖ% 0
(
ÖÖ0 1
Guid
ÖÖ1 5
id
ÖÖ6 8
,
ÖÖ8 9
	IFormFile
ÖÖ: C
?
ÖÖC D
file
ÖÖE I
,
ÖÖI J
CancellationToken
ÖÖK \
cancellationToken
ÖÖ] n
)
ÖÖn o
{
ÜÜ 
var
áá 

uploadFile
áá 
=
áá 
file
áá 
??
áá  
(
áá! "
Request
áá" )
.
áá) * 
HasFormContentType
áá* <
&&
áá= ?
Request
áá@ G
.
ááG H
Form
ááH L
.
ááL M
Files
ááM R
.
ááR S
Count
ááS X
>
ááY Z
$num
áá[ \
?
áá] ^
Request
áá_ f
.
ááf g
Form
áág k
.
áák l
Files
áál q
[
ááq r
$num
áár s
]
áás t
:
ááu v
null
ááw {
)
áá{ |
;
áá| }
if
ââ 

(
ââ 

uploadFile
ââ 
==
ââ 
null
ââ 
||
ââ !

uploadFile
ââ" ,
.
ââ, -
Length
ââ- 3
==
ââ4 6
$num
ââ7 8
)
ââ8 9
{
ää 	
return
ãã 

BadRequest
ãã 
(
ãã 
new
ãã !
{
ãã" #
message
ãã$ +
=
ãã, -
$str
ãã. Q
}
ããR S
)
ããS T
;
ããT U
}
åå 	
var
éé 
ext
éé 
=
éé 
Path
éé 
.
éé 
GetExtension
éé #
(
éé# $

uploadFile
éé$ .
.
éé. /
FileName
éé/ 7
)
éé7 8
.
éé8 9
ToLowerInvariant
éé9 I
(
ééI J
)
ééJ K
;
ééK L
if
èè 

(
èè 
!
èè 
AllowedExtensions
èè 
.
èè 
Contains
èè '
(
èè' (
ext
èè( +
)
èè+ ,
)
èè, -
{
êê 	
return
ëë 

BadRequest
ëë 
(
ëë 
new
ëë !
{
ëë" #
message
ëë$ +
=
ëë, -
$str
ëë. 
}ëëÄ Å
)ëëÅ Ç
;ëëÇ É
}
íí 	
var
îî 
receiptsDir
îî 
=
îî 
Path
îî 
.
îî 
Combine
îî &
(
îî& '
_environment
îî' 3
.
îî3 4
ContentRootPath
îî4 C
,
îîC D
$str
îîE O
,
îîO P
$str
îîQ [
)
îî[ \
;
îî\ ]
if
ïï 

(
ïï 
!
ïï 
	Directory
ïï 
.
ïï 
Exists
ïï 
(
ïï 
receiptsDir
ïï )
)
ïï) *
)
ïï* +
{
ññ 	
	Directory
óó 
.
óó 
CreateDirectory
óó %
(
óó% &
receiptsDir
óó& 1
)
óó1 2
;
óó2 3
}
òò 	
var
öö 
safeFileName
öö 
=
öö 
$"
öö 
{
öö 
Guid
öö "
.
öö" #
NewGuid
öö# *
(
öö* +
)
öö+ ,
:
öö, -
$str
öö- .
}
öö. /
$str
öö/ 0
{
öö0 1
Path
öö1 5
.
öö5 6)
GetFileNameWithoutExtension
öö6 Q
(
ööQ R

uploadFile
ööR \
.
öö\ ]
FileName
öö] e
)
ööe f
.
ööf g
Replace
öög n
(
öön o
$str
ööo r
,
öör s
$str
ööt w
)
ööw x
}
ööx y
{
ööy z
ext
ööz }
}
öö} ~
"
öö~ 
;öö Ä
var
õõ 
filePath
õõ 
=
õõ 
Path
õõ 
.
õõ 
Combine
õõ #
(
õõ# $
receiptsDir
õõ$ /
,
õõ/ 0
safeFileName
õõ1 =
)
õõ= >
;
õõ> ?
await
ùù 
using
ùù 
(
ùù 
var
ùù 
stream
ùù 
=
ùù  !
new
ùù" %

FileStream
ùù& 0
(
ùù0 1
filePath
ùù1 9
,
ùù9 :
FileMode
ùù; C
.
ùùC D
Create
ùùD J
)
ùùJ K
)
ùùK L
{
ûû 	
await
üü 

uploadFile
üü 
.
üü 
CopyToAsync
üü (
(
üü( )
stream
üü) /
,
üü/ 0
cancellationToken
üü1 B
)
üüB C
;
üüC D
}
†† 	
var
¢¢ 
relativeUrl
¢¢ 
=
¢¢ 
$"
¢¢ 
$str
¢¢ 0
{
¢¢0 1
safeFileName
¢¢1 =
}
¢¢= >
"
¢¢> ?
;
¢¢? @
var
££ 
(
££ 
success
££ 
,
££ 
message
££ 
,
££ 
	proofPath
££ (
)
££( )
=
££* +
await
££, 1
_expenseService
££2 A
.
££A B
AttachProofAsync
££B R
(
££R S
id
££S U
,
££U V
relativeUrl
££W b
,
££b c
cancellationToken
££d u
)
££u v
;
££v w
if
§§ 

(
§§ 
!
§§ 
success
§§ 
)
§§ 
{
•• 	
return
¶¶ 
NotFound
¶¶ 
(
¶¶ 
new
¶¶ 
{
¶¶  !
message
¶¶" )
}
¶¶* +
)
¶¶+ ,
;
¶¶, -
}
ßß 	
return
©© 
Ok
©© 
(
©© 
new
©© 
{
™™ 	
success
´´ 
=
´´ 
true
´´ 
,
´´ 
	proofPath
¨¨ 
=
¨¨ 
relativeUrl
¨¨ #
,
¨¨# $
fileName
≠≠ 
=
≠≠ 

uploadFile
≠≠ !
.
≠≠! "
FileName
≠≠" *
,
≠≠* +
fileUrl
ÆÆ 
=
ÆÆ 
$"
ÆÆ 
{
ÆÆ 
Request
ÆÆ  
.
ÆÆ  !
Scheme
ÆÆ! '
}
ÆÆ' (
$str
ÆÆ( +
{
ÆÆ+ ,
Request
ÆÆ, 3
.
ÆÆ3 4
Host
ÆÆ4 8
}
ÆÆ8 9
{
ÆÆ9 :
relativeUrl
ÆÆ: E
}
ÆÆE F
"
ÆÆF G
,
ÆÆG H
message
ØØ 
}
∞∞ 	
)
∞∞	 

;
∞∞
 
}
±± 
[
≥≥ 
HttpGet
≥≥ 
(
≥≥ 
$str
≥≥ 
)
≥≥  
]
≥≥  !
[
¥¥ 
	Authorize
¥¥ 
(
¥¥ 
Roles
¥¥ 
=
¥¥ 
$str
¥¥ 7
)
¥¥7 8
]
¥¥8 9
public
µµ 

IActionResult
µµ 
GetProofFile
µµ %
(
µµ% &
string
µµ& ,
fileName
µµ- 5
)
µµ5 6
{
∂∂ 
if
∑∑ 

(
∑∑ 
string
∑∑ 
.
∑∑  
IsNullOrWhiteSpace
∑∑ %
(
∑∑% &
fileName
∑∑& .
)
∑∑. /
)
∑∑/ 0
{
∏∏ 	
return
ππ 

BadRequest
ππ 
(
ππ 
new
ππ !
{
ππ" #
message
ππ$ +
=
ππ, -
$str
ππ. F
}
ππG H
)
ππH I
;
ππI J
}
∫∫ 	
var
ºº 
	cleanName
ºº 
=
ºº 
Path
ºº 
.
ºº 
GetFileName
ºº (
(
ºº( )
fileName
ºº) 1
)
ºº1 2
;
ºº2 3
var
ΩΩ 
receiptsDir
ΩΩ 
=
ΩΩ 
Path
ΩΩ 
.
ΩΩ 
Combine
ΩΩ &
(
ΩΩ& '
_environment
ΩΩ' 3
.
ΩΩ3 4
ContentRootPath
ΩΩ4 C
,
ΩΩC D
$str
ΩΩE O
,
ΩΩO P
$str
ΩΩQ [
)
ΩΩ[ \
;
ΩΩ\ ]
var
ææ 
filePath
ææ 
=
ææ 
Path
ææ 
.
ææ 
Combine
ææ #
(
ææ# $
receiptsDir
ææ$ /
,
ææ/ 0
	cleanName
ææ1 :
)
ææ: ;
;
ææ; <
if
¿¿ 

(
¿¿ 
!
¿¿ 
System
¿¿ 
.
¿¿ 
IO
¿¿ 
.
¿¿ 
File
¿¿ 
.
¿¿ 
Exists
¿¿ "
(
¿¿" #
filePath
¿¿# +
)
¿¿+ ,
)
¿¿, -
{
¡¡ 	
return
¬¬ 
NotFound
¬¬ 
(
¬¬ 
new
¬¬ 
{
¬¬  !
message
¬¬" )
=
¬¬* +
$str
¬¬, I
}
¬¬J K
)
¬¬K L
;
¬¬L M
}
√√ 	
var
≈≈ 
ext
≈≈ 
=
≈≈ 
Path
≈≈ 
.
≈≈ 
GetExtension
≈≈ #
(
≈≈# $
	cleanName
≈≈$ -
)
≈≈- .
.
≈≈. /
ToLowerInvariant
≈≈/ ?
(
≈≈? @
)
≈≈@ A
;
≈≈A B
var
∆∆ 
contentType
∆∆ 
=
∆∆ 
ext
∆∆ 
switch
∆∆ $
{
«« 	
$str
»» 
=>
»» 
$str
»» '
,
»»' (
$str
…… 
=>
…… 
$str
…… !
,
……! "
$str
   
or
   
$str
   
=>
    
$str
  ! -
,
  - .
$str
ÀÀ 
=>
ÀÀ 
$str
ÀÀ #
,
ÀÀ# $
_
ÃÃ 
=>
ÃÃ 
$str
ÃÃ +
}
ÕÕ 	
;
ÕÕ	 

var
œœ 
stream
œœ 
=
œœ 
new
œœ 

FileStream
œœ #
(
œœ# $
filePath
œœ$ ,
,
œœ, -
FileMode
œœ. 6
.
œœ6 7
Open
œœ7 ;
,
œœ; <

FileAccess
œœ= G
.
œœG H
Read
œœH L
,
œœL M
	FileShare
œœN W
.
œœW X
Read
œœX \
)
œœ\ ]
;
œœ] ^
return
–– 
File
–– 
(
–– 
stream
–– 
,
–– 
contentType
–– '
,
––' (
	cleanName
––) 2
,
––2 3#
enableRangeProcessing
––4 I
:
––I J
true
––K O
)
––O P
;
––P Q
}
—— 
}““ â/
sC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\PoliciesController.cs
	namespace

 	
	VoyageHub


 
.

 
Api

 
.

 
Controllers

 #
;

# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

]
 
public 
class 
PoliciesController 
:  !
ControllerBase" 0
{ 
private 
readonly 
IPolicyService #
_policyService$ 2
;2 3
public 

PoliciesController 
( 
IPolicyService ,
policyService- :
): ;
{ 
_policyService 
= 
policyService &
;& '
} 
[ 
HttpGet 
] 
[ 
	Authorize 
( 
Roles 
= 
$str 7
)7 8
]8 9
public 

async 
Task 
< 
ActionResult "
<" #
List# '
<' (
PolicyConfigDto( 7
>7 8
>8 9
>9 :
GetPolicies; F
(F G
CancellationTokenG X
cancellationTokenY j
)j k
{ 
var 
policies 
= 
await 
_policyService +
.+ ,
GetPoliciesAsync, <
(< =
cancellationToken= N
)N O
;O P
return 
Ok 
( 
policies 
) 
; 
} 
[   
HttpPut   
(   
$str   
)   
]   
[!! 
	Authorize!! 
(!! 
Roles!! 
=!! 
$str!! 
)!! 
]!!  
public"" 

async"" 
Task"" 
<"" 
IActionResult"" #
>""# $
UpdatePolicy""% 1
(""1 2
Guid""2 6
id""7 9
,""9 :
[""; <
FromBody""< D
]""D E
UpdatePolicyDto""F U
dto""V Y
,""Y Z
CancellationToken""[ l
cancellationToken""m ~
)""~ 
{## 
var$$ 
($$ 
success$$ 
,$$ 
message$$ 
)$$ 
=$$  
await$$! &
_policyService$$' 5
.$$5 6
UpdatePolicyAsync$$6 G
($$G H
id$$H J
,$$J K
dto$$L O
,$$O P
cancellationToken$$Q b
)$$b c
;$$c d
if%% 

(%% 
!%% 
success%% 
)%% 
{&& 	
return'' 
NotFound'' 
('' 
new'' 
{''  !
message''" )
}''* +
)''+ ,
;'', -
}(( 	
return** 
Ok** 
(** 
new** 
{** 
success** 
=**  !
true**" &
,**& '
message**( /
}**0 1
)**1 2
;**2 3
}++ 
[-- 
HttpPost-- 
]-- 
[.. 
	Authorize.. 
(.. 
Roles.. 
=.. 
$str.. 
).. 
]..  
public// 

async// 
Task// 
<// 
ActionResult// "
<//" #
PolicyConfigDto//# 2
>//2 3
>//3 4
CreatePolicy//5 A
(//A B
[//B C
FromBody//C K
]//K L
CreatePolicyDto//M \
dto//] `
,//` a
CancellationToken//b s
cancellationToken	//t Ö
)
//Ö Ü
{00 
var11 
(11 
success11 
,11 
message11 
,11 
policy11 %
)11% &
=11' (
await11) .
_policyService11/ =
.11= >
CreatePolicyAsync11> O
(11O P
dto11P S
,11S T
cancellationToken11U f
)11f g
;11g h
if22 

(22 
!22 
success22 
)22 
{33 	
return44 

BadRequest44 
(44 
new44 !
{44" #
message44$ +
}44, -
)44- .
;44. /
}55 	
return77 
Ok77 
(77 
policy77 
)77 
;77 
}88 
[:: 

HttpDelete:: 
(:: 
$str:: 
):: 
]:: 
[;; 
	Authorize;; 
(;; 
Roles;; 
=;; 
$str;; 
);; 
];;  
public<< 

async<< 
Task<< 
<<< 
IActionResult<< #
><<# $
DeletePolicy<<% 1
(<<1 2
Guid<<2 6
id<<7 9
,<<9 :
CancellationToken<<; L
cancellationToken<<M ^
)<<^ _
{== 
var>> 
(>> 
success>> 
,>> 
message>> 
)>> 
=>>  
await>>! &
_policyService>>' 5
.>>5 6
DeletePolicyAsync>>6 G
(>>G H
id>>H J
,>>J K
cancellationToken>>L ]
)>>] ^
;>>^ _
if?? 

(?? 
!?? 
success?? 
)?? 
{@@ 	
returnAA 
NotFoundAA 
(AA 
newAA 
{AA  !
messageAA" )
}AA* +
)AA+ ,
;AA, -
}BB 	
returnDD 
OkDD 
(DD 
newDD 
{DD 
successDD 
=DD  !
trueDD" &
,DD& '
messageDD( /
}DD0 1
)DD1 2
;DD2 3
}EE 
}FF ñb
oC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\AuthController.cs
	namespace 	
	VoyageHub
 
. 
Api 
. 
Controllers #
;# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
public 
class 
AuthController 
: 
ControllerBase ,
{ 
private 
readonly 
IAuthService !
_authService" .
;. /
private 
readonly 
ILogger 
< 
AuthController +
>+ ,
_logger- 4
;4 5
private 
readonly 
IWebHostEnvironment (
_environment) 5
;5 6
private 
readonly 
IConfiguration #
_configuration$ 2
;2 3
public 

AuthController 
( 
IAuthService 
authService  
,  !
ILogger 
< 
AuthController 
> 
logger  &
,& '
IWebHostEnvironment 
environment '
,' (
IConfiguration 
configuration $
)$ %
{ 
_authService 
= 
authService "
;" #
_logger   
=   
logger   
;   
_environment!! 
=!! 
environment!! "
;!!" #
_configuration"" 
="" 
configuration"" &
;""& '
}## 
[%% 
HttpPost%% 
(%% 
$str%% 
)%% 
]%% 
[&& 
AllowAnonymous&& 
]&& 
public'' 

async'' 
Task'' 
<'' 
ActionResult'' "
<''" #
AuthResponseDto''# 2
>''2 3
>''3 4
Login''5 :
('': ;
[''; <
FromBody''< D
]''D E
LoginDto''F N
dto''O R
,''R S
CancellationToken''T e
cancellationToken''f w
)''w x
{(( 
try)) 
{** 	
var++ 
(++ 

statusCode++ 
,++ 
response++ %
)++% &
=++' (
await++) .
_authService++/ ;
.++; <

LoginAsync++< F
(++F G
dto++G J
,++J K
cancellationToken++L ]
)++] ^
;++^ _
return,, 

StatusCode,, 
(,, 

statusCode,, (
,,,( )
response,,* 2
),,2 3
;,,3 4
}-- 	
catch.. 
(.. 
	Exception.. 
ex.. 
).. 
{// 	
_logger00 
.00 
LogError00 
(00 
ex00 
,00  
$str00! 5
)005 6
;006 7
return11 

StatusCode11 
(11 
$num11 !
,11! "
new11# &
AuthResponseDto11' 6
{22 
Success33 
=33 
false33 
,33  
Message44 
=44 
$"44 
$str44 ,
{44, -
ex44- /
.44/ 0
Message440 7
}447 8
"448 9
}55 
)55 
;55 
}66 	
}77 
[99 
HttpGet99 
(99 
$str99 
)99 
]99 
[:: 
AllowAnonymous:: 
]:: 
public;; 

async;; 
Task;; 
<;; 
ActionResult;; "
<;;" #
List;;# '
<;;' (
UserProfileDto;;( 6
>;;6 7
>;;7 8
>;;8 9
GetQuickPersonas;;: J
(;;J K
CancellationToken;;K \
cancellationToken;;] n
);;n o
{<< 
var== 
users== 
=== 
await== 
_authService== &
.==& '
GetPersonasAsync==' 7
(==7 8
cancellationToken==8 I
)==I J
;==J K
return>> 
Ok>> 
(>> 
users>> 
)>> 
;>> 
}?? 
[AA 
HttpPutAA 
(AA 
$strAA 
)AA 
]AA 
[BB 
	AuthorizeBB 
]BB 
publicCC 

asyncCC 
TaskCC 
<CC 
ActionResultCC "
<CC" #
AuthResponseDtoCC# 2
>CC2 3
>CC3 4
UpdateProfileCC5 B
(CCB C
[CCC D
FromBodyCCD L
]CCL M
UpdateProfileDtoCCN ^
dtoCC_ b
,CCb c
CancellationTokenCCd u
cancellationToken	CCv á
)
CCá à
{DD 
tryEE 
{FF 	
varGG 
(GG 

statusCodeGG 
,GG 
responseGG %
)GG% &
=GG' (
awaitGG) .
_authServiceGG/ ;
.GG; <
UpdateProfileAsyncGG< N
(GGN O
dtoGGO R
,GGR S
cancellationTokenGGT e
)GGe f
;GGf g
returnHH 

StatusCodeHH 
(HH 

statusCodeHH (
,HH( )
responseHH* 2
)HH2 3
;HH3 4
}II 	
catchJJ 
(JJ 
	ExceptionJJ 
exJJ 
)JJ 
{KK 	
_loggerLL 
.LL 
LogErrorLL 
(LL 
exLL 
,LL  
$strLL! 9
)LL9 :
;LL: ;
returnMM 

StatusCodeMM 
(MM 
$numMM !
,MM! "
newMM# &
AuthResponseDtoMM' 6
{NN 
SuccessOO 
=OO 
falseOO 
,OO  
MessagePP 
=PP 
$"PP 
$strPP ,
{PP, -
exPP- /
.PP/ 0
MessagePP0 7
}PP7 8
"PP8 9
}QQ 
)QQ 
;QQ 
}RR 	
}SS 
[UU 
HttpPostUU 
(UU 
$strUU 
)UU  
]UU  !
[VV 
	AuthorizeVV 
]VV 
publicWW 

asyncWW 
TaskWW 
<WW 
ActionResultWW "
<WW" #
AuthResponseDtoWW# 2
>WW2 3
>WW3 4
ChangePasswordWW5 C
(WWC D
[WWD E
FromBodyWWE M
]WWM N
ChangePasswordDtoWWO `
dtoWWa d
,WWd e
CancellationTokenWWf w
cancellationToken	WWx â
)
WWâ ä
{XX 
tryYY 
{ZZ 	
var[[ 
([[ 

statusCode[[ 
,[[ 
response[[ %
)[[% &
=[[' (
await[[) .
_authService[[/ ;
.[[; <
ChangePasswordAsync[[< O
([[O P
dto[[P S
,[[S T
cancellationToken[[U f
)[[f g
;[[g h
return\\ 

StatusCode\\ 
(\\ 

statusCode\\ (
,\\( )
response\\* 2
)\\2 3
;\\3 4
}]] 	
catch^^ 
(^^ 
	Exception^^ 
ex^^ 
)^^ 
{__ 	
_logger`` 
.`` 
LogError`` 
(`` 
ex`` 
,``  
$str``! :
)``: ;
;``; <
returnaa 

StatusCodeaa 
(aa 
$numaa !
,aa! "
newaa# &
AuthResponseDtoaa' 6
{bb 
Successcc 
=cc 
falsecc 
,cc  
Messagedd 
=dd 
$"dd 
$strdd ,
{dd, -
exdd- /
.dd/ 0
Messagedd0 7
}dd7 8
"dd8 9
}ee 
)ee 
;ee 
}ff 	
}gg 
[ii 
HttpPostii 
(ii 
$strii 
)ii  
]ii  !
[jj 
AllowAnonymousjj 
]jj 
publickk 

asynckk 
Taskkk 
<kk 
ActionResultkk "
<kk" #%
ForgotPasswordResponseDtokk# <
>kk< =
>kk= >
ForgotPasswordkk? M
(kkM N
[kkN O
FromBodykkO W
]kkW X
ForgotPasswordDtokkY j
dtokkk n
,kkn o
CancellationToken	kkp Å
cancellationToken
kkÇ ì
)
kkì î
{ll 
trymm 
{nn 	
varoo 
frontendBaseUrloo 
=oo  !
_configurationoo" 0
[oo0 1
$stroo1 >
]oo> ?
;oo? @
varpp 
(pp 

statusCodepp 
,pp 
responsepp %
)pp% &
=pp' (
awaitpp) .
_authServicepp/ ;
.pp; <
ForgotPasswordAsyncpp< O
(ppO P
dtoqq 
,qq 
_environmentrr 
.rr 
IsDevelopmentrr *
(rr* +
)rr+ ,
,rr, -
frontendBaseUrlss 
,ss  
cancellationTokentt !
)tt! "
;tt" #
returnvv 

StatusCodevv 
(vv 

statusCodevv (
,vv( )
responsevv* 2
)vv2 3
;vv3 4
}ww 	
catchxx 
(xx 
	Exceptionxx 
exxx 
)xx 
{yy 	
_loggerzz 
.zz 
LogErrorzz 
(zz 
exzz 
,zz  
$strzz! G
)zzG H
;zzH I
return{{ 

StatusCode{{ 
({{ 
$num{{ !
,{{! "
new{{# &%
ForgotPasswordResponseDto{{' @
{|| 
Success}} 
=}} 
false}} 
,}}  
Message~~ 
=~~ 
$"~~ 
$str~~ ,
{~~, -
ex~~- /
.~~/ 0
Message~~0 7
}~~7 8
"~~8 9
} 
) 
; 
}
ÄÄ 	
}
ÅÅ 
[
ÉÉ 
HttpPost
ÉÉ 
(
ÉÉ 
$str
ÉÉ "
)
ÉÉ" #
]
ÉÉ# $
[
ÑÑ 
AllowAnonymous
ÑÑ 
]
ÑÑ 
public
ÖÖ 

async
ÖÖ 
Task
ÖÖ 
<
ÖÖ 
ActionResult
ÖÖ "
<
ÖÖ" #
AuthResponseDto
ÖÖ# 2
>
ÖÖ2 3
>
ÖÖ3 4
VerifyResetToken
ÖÖ5 E
(
ÖÖE F
[
ÖÖF G
FromBody
ÖÖG O
]
ÖÖO P!
VerifyResetTokenDto
ÖÖQ d
dto
ÖÖe h
,
ÖÖh i
CancellationToken
ÖÖj { 
cancellationTokenÖÖ| ç
)ÖÖç é
{
ÜÜ 
var
áá 
(
áá 

statusCode
áá 
,
áá 
response
áá !
)
áá! "
=
áá# $
await
áá% *
_authService
áá+ 7
.
áá7 8#
VerifyResetTokenAsync
áá8 M
(
ááM N
dto
ááN Q
,
ááQ R
cancellationToken
ááS d
)
áád e
;
ááe f
return
àà 

StatusCode
àà 
(
àà 

statusCode
àà $
,
àà$ %
response
àà& .
)
àà. /
;
àà/ 0
}
ââ 
[
ãã 
HttpPost
ãã 
(
ãã 
$str
ãã 
)
ãã 
]
ãã  
[
åå 
AllowAnonymous
åå 
]
åå 
public
çç 

async
çç 
Task
çç 
<
çç 
ActionResult
çç "
<
çç" #
AuthResponseDto
çç# 2
>
çç2 3
>
çç3 4
ResetPassword
çç5 B
(
ççB C
[
ççC D
FromBody
ççD L
]
ççL M
ResetPasswordDto
ççN ^
dto
çç_ b
,
ççb c
CancellationToken
ççd u 
cancellationTokenççv á
)ççá à
{
éé 
var
èè 
(
èè 

statusCode
èè 
,
èè 
response
èè !
)
èè! "
=
èè# $
await
èè% *
_authService
èè+ 7
.
èè7 8 
ResetPasswordAsync
èè8 J
(
èèJ K
dto
èèK N
,
èèN O
cancellationToken
èèP a
)
èèa b
;
èèb c
return
êê 

StatusCode
êê 
(
êê 

statusCode
êê $
,
êê$ %
response
êê& .
)
êê. /
;
êê/ 0
}
ëë 
}íí Ò
tC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\AuditLogsController.cs
	namespace		 	
	VoyageHub		
 
.		 
Api		 
.		 
Controllers		 #
;		# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

(
 
Roles 
= 
$str 
) 
] 
public 
class 
AuditLogsController  
:! "
ControllerBase# 1
{ 
private 
readonly 
IAuditService "
_auditService# 0
;0 1
public 

AuditLogsController 
( 
IAuditService ,
auditService- 9
)9 :
{ 
_auditService 
= 
auditService $
;$ %
} 
[ 
HttpGet 
] 
public 

async 
Task 
< 
ActionResult "
<" #
List# '
<' (
AuditLogDto( 3
>3 4
>4 5
>5 6
GetLogs7 >
(> ?
[ 	
	FromQuery	 
] 
string 
? 
severity $
,$ %
[ 	
	FromQuery	 
] 
string 
? 

entityType &
,& '
[ 	
	FromQuery	 
] 
int 
limit 
= 
$num  "
," #
CancellationToken 
cancellationToken +
=, -
default. 5
)5 6
{ 
var 
logs 
= 
await 
_auditService &
.& '
GetLogsAsync' 3
(3 4
severity4 <
,< =

entityType> H
,H I
limitJ O
,O P
cancellationTokenQ b
)b c
;c d
return 
Ok 
( 
logs 
) 
; 
}   
}!! ú
tC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\ApprovalsController.cs
	namespace		 	
	VoyageHub		
 
.		 
Api		 
.		 
Controllers		 #
;		# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

(
 
Roles 
= 
$str *
)* +
]+ ,
public 
class 
ApprovalsController  
:! "
ControllerBase# 1
{ 
private 
readonly 
IApprovalService %
_approvalService& 6
;6 7
public 

ApprovalsController 
( 
IApprovalService /
approvalService0 ?
)? @
{ 
_approvalService 
= 
approvalService *
;* +
} 
[ 
HttpGet 
( 
$str 
) 
] 
public 

async 
Task 
< 
ActionResult "
<" ##
ApprovalQueueSummaryDto# :
>: ;
>; <
GetQueue= E
(E F
[ 	
	FromQuery	 
] 
Guid 
? 

approverId $
,$ %
[ 	
	FromQuery	 
] 
string 
? 
role  
,  !
CancellationToken 
cancellationToken +
)+ ,
{ 
var 
summary 
= 
await 
_approvalService ,
., -
GetQueueAsync- :
(: ;

approverId; E
,E F
roleG K
,K L
cancellationTokenM ^
)^ _
;_ `
return 
Ok 
( 
summary 
) 
; 
} 
[!! 
HttpPost!! 
(!! 
$str!! 
)!!  
]!!  !
public"" 

async"" 
Task"" 
<"" 
IActionResult"" #
>""# $

TakeAction""% /
(""/ 0
Guid""0 4
stepId""5 ;
,""; <
[""= >
FromBody""> F
]""F G
ApprovalActionDto""H Y
dto""Z ]
,""] ^
CancellationToken""_ p
cancellationToken	""q Ç
)
""Ç É
{## 
var$$ 
($$ 
success$$ 
,$$ 
message$$ 
)$$ 
=$$  
await$$! &
_approvalService$$' 7
.$$7 8
TakeActionAsync$$8 G
($$G H
stepId$$H N
,$$N O
dto$$P S
,$$S T
cancellationToken$$U f
)$$f g
;$$g h
if%% 

(%% 
!%% 
success%% 
)%% 
{&& 	
return'' 
NotFound'' 
('' 
new'' 
{''  !
message''" )
}''* +
)''+ ,
;'', -
}(( 	
return** 
Ok** 
(** 
new** 
{** 
success** 
=**  !
true**" &
,**& '
message**( /
}**0 1
)**1 2
;**2 3
}++ 
},, ∑
tC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\Controllers\AnalyticsController.cs
	namespace		 	
	VoyageHub		
 
.		 
Api		 
.		 
Controllers		 #
;		# $
[ 
ApiController 
] 
[ 
Route 
( 
$str 
) 
] 
[ 
	Authorize 

(
 
Roles 
= 
$str *
)* +
]+ ,
public 
class 
AnalyticsController  
:! "
ControllerBase# 1
{ 
private 
readonly 
IAnalyticsService &
_analyticsService' 8
;8 9
public 

AnalyticsController 
( 
IAnalyticsService 0
analyticsService1 A
)A B
{ 
_analyticsService 
= 
analyticsService ,
;, -
} 
[ 
HttpGet 
( 
$str 
) 
] 
public 

async 
Task 
< 
ActionResult "
<" # 
AnalyticsOverviewDto# 7
>7 8
>8 9
GetOverview: E
(E F
CancellationTokenF W
cancellationTokenX i
)i j
{ 
var 
overview 
= 
await 
_analyticsService .
.. /
GetOverviewAsync/ ?
(? @
cancellationToken@ Q
)Q R
;R S
return 
Ok 
( 
overview 
) 
; 
} 
[ 
HttpGet 
( 
$str 
)  
]  !
public 

async 
Task 
< 
ActionResult "
<" #
List# '
<' ("
EmployeeSpendReportDto( >
>> ?
>? @
>@ A
GetEmployeeReportsB T
(T U
CancellationTokenU f
cancellationTokeng x
)x y
{   
var!! 
reports!! 
=!! 
await!! 
_analyticsService!! -
.!!- .'
GetEmployeeSpendReportAsync!!. I
(!!I J
cancellationToken!!J [
)!![ \
;!!\ ]
return"" 
Ok"" 
("" 
reports"" 
)"" 
;"" 
}## 
[%% 
HttpGet%% 
(%% 
$str%% 
)%% 
]%% 
public&& 

async&& 
Task&& 
<&& 
ActionResult&& "
<&&" # 
CostSummaryReportDto&&# 7
>&&7 8
>&&8 9
GetCostReports&&: H
(&&H I
CancellationToken&&I Z
cancellationToken&&[ l
)&&l m
{'' 
var(( 
report(( 
=(( 
await(( 
_analyticsService(( ,
.((, -%
GetCostSummaryReportAsync((- F
(((F G
cancellationToken((G X
)((X Y
;((Y Z
return)) 
Ok)) 
()) 
report)) 
))) 
;)) 
}** 
}++ 