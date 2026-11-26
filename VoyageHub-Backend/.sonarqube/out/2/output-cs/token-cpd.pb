░!
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
}++ Ъа
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
$str	, Ь
)
Ь Э
;
Э Ю
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
$str	!!. Е
{
!!Е Ж
jwtKey
!!Ж М
.
!!М Н
Length
!!Н У
}
!!У Ф
$str
!!Ф Ц
"
!!Ц Ч
)
!!Ч Ш
;
!!Ш Щ
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
$str	(( Е
;
((Е Ж
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
АА 
=
АА 

envOrigins
АА  
;
АА  !
}
ББ 
else
ВВ 
{
ГГ 
corsOrigins
ДД 
=
ДД 
corsOrigins
ДД !
.
ДД! "
Where
ДД" '
(
ДД' (
o
ДД( )
=>
ДД* ,
!
ДД- .
o
ДД. /
.
ДД/ 0
Contains
ДД0 8
(
ДД8 9
$str
ДД9 D
,
ДДD E
StringComparison
ДДF V
.
ДДV W
OrdinalIgnoreCase
ДДW h
)
ДДh i
&&
ДДj l
!
ДДm n
o
ДДn o
.
ДДo p
Contains
ДДp x
(
ДДx y
$strДДy Д
)ДДД Е
)ДДЕ Ж
.ДДЖ З
ToArrayДДЗ О
(ДДО П
)ДДП Р
;ДДР С
}
ЕЕ 
}ЖЖ 
builderИИ 
.
ИИ 
Services
ИИ 
.
ИИ 
AddCors
ИИ 
(
ИИ 
options
ИИ  
=>
ИИ! #
{ЙЙ 
options
КК 
.
КК 
	AddPolicy
КК 
(
КК 
$str
КК '
,
КК' (
policy
КК) /
=>
КК0 2
{
ЛЛ 
if
ММ 

(
ММ 
corsOrigins
ММ 
.
ММ 
Length
ММ 
>
ММ  
$num
ММ! "
)
ММ" #
{
НН 	
policy
ОО 
.
ОО 
WithOrigins
ОО 
(
ОО 
corsOrigins
ОО *
)
ОО* +
;
ОО+ ,
}
ПП 	
policy
РР 
.
РР 
AllowAnyHeader
РР 
(
РР 
)
РР 
.
СС 
AllowAnyMethod
СС 
(
СС 
)
СС 
.
ТТ 
AllowCredentials
ТТ 
(
ТТ 
)
ТТ 
;
ТТ  
}
УУ 
)
УУ 
;
УУ 
}ФФ 
)
ФФ 
;
ФФ 
builderЧЧ 
.
ЧЧ 
Services
ЧЧ 
.
ЧЧ %
AddEndpointsApiExplorer
ЧЧ (
(
ЧЧ( )
)
ЧЧ) *
;
ЧЧ* +
builderШШ 
.
ШШ 
Services
ШШ 
.
ШШ 
AddSwaggerGen
ШШ 
(
ШШ 
options
ШШ &
=>
ШШ' )
{ЩЩ 
options
ЪЪ 
.
ЪЪ 

SwaggerDoc
ЪЪ 
(
ЪЪ 
$str
ЪЪ 
,
ЪЪ 
new
ЪЪ  
OpenApiInfo
ЪЪ! ,
{
ЫЫ 
Title
ЬЬ 
=
ЬЬ 
$str
ЬЬ 
,
ЬЬ  
Version
ЭЭ 
=
ЭЭ 
$str
ЭЭ 
,
ЭЭ 
Description
ЮЮ 
=
ЮЮ 
$str
ЮЮ j
}
ЯЯ 
)
ЯЯ 
;
ЯЯ 
options
бб 
.
бб #
AddSecurityDefinition
бб !
(
бб! "
$str
бб" *
,
бб* +
new
бб, /#
OpenApiSecurityScheme
бб0 E
{
вв 
Name
гг 
=
гг 
$str
гг 
,
гг 
Description
дд 
=
дд 
$str
дд J
,
ддJ K
In
ее 

=
ее 
ParameterLocation
ее 
.
ее 
Header
ее %
,
ее% &
Type
жж 
=
жж  
SecuritySchemeType
жж !
.
жж! "
Http
жж" &
,
жж& '
Scheme
зз 
=
зз 
$str
зз 
,
зз 
BearerFormat
ии 
=
ии 
$str
ии 
}
йй 
)
йй 
;
йй 
options
лл 
.
лл $
AddSecurityRequirement
лл "
(
лл" #
_
лл# $
=>
лл% '
new
лл( +(
OpenApiSecurityRequirement
лл, F
{
мм 
{
нн 	
new
оо ,
OpenApiSecuritySchemeReference
оо .
(
оо. /
$str
оо/ 7
)
оо7 8
,
оо8 9
new
пп 
List
пп 
<
пп 
string
пп 
>
пп 
(
пп 
)
пп 
}
░░ 	
}
▒▒ 
)
▒▒ 
;
▒▒ 
}▓▓ 
)
▓▓ 
;
▓▓ 
var┤┤ 
app
┤┤ 
=
┤┤ 	
builder
┤┤
 
.
┤┤ 
Build
┤┤ 
(
┤┤ 
)
┤┤ 
;
┤┤ 
var╢╢ 
swaggerEnabled
╢╢ 
=
╢╢ 
builder
╢╢ 
.
╢╢ 
Configuration
╢╢ *
.
╢╢* +
GetValue
╢╢+ 3
<
╢╢3 4
bool
╢╢4 8
>
╢╢8 9
(
╢╢9 :
$str
╢╢: K
,
╢╢K L
builder
╢╢M T
.
╢╢T U
Environment
╢╢U `
.
╢╢` a
IsDevelopment
╢╢a n
(
╢╢n o
)
╢╢o p
)
╢╢p q
;
╢╢q r
if╕╕ 
(
╕╕ 
swaggerEnabled
╕╕ 
)
╕╕ 
{╣╣ 
app
║║ 
.
║║ 

UseSwagger
║║ 
(
║║ 
)
║║ 
;
║║ 
app
╗╗ 
.
╗╗ 
UseSwaggerUI
╗╗ 
(
╗╗ 
c
╗╗ 
=>
╗╗ 
{
╝╝ 
c
╜╜ 	
.
╜╜	 

SwaggerEndpoint
╜╜
 
(
╜╜ 
$str
╜╜ 4
,
╜╜4 5
$str
╜╜6 H
)
╜╜H I
;
╜╜I J
c
╛╛ 	
.
╛╛	 

RoutePrefix
╛╛
 
=
╛╛ 
$str
╛╛ !
;
╛╛! "
}
┐┐ 
)
┐┐ 
;
┐┐ 
}└└ 
app├├ 
.
├├ 
MapGet
├├ 

(
├├
 
$str
├├ 
,
├├ 
(
├├ 
)
├├ 
=>
├├ 
swaggerEnabled
├├ $
?
── 
Results
── 
.
── 
Redirect
── 
(
── 
$str
── !
)
──! "
:
┼┼ 
Results
┼┼ 
.
┼┼ 
Text
┼┼ 
(
┼┼ 
$str
┼┼ .
,
┼┼. /
$str
┼┼0 <
)
┼┼< =
)
┼┼= >
;
┼┼> ?
app╟╟ 
.
╟╟ 
UseCors
╟╟ 
(
╟╟ 
$str
╟╟ 
)
╟╟ 
;
╟╟ 
app╔╔ 
.
╔╔ !
UseHttpsRedirection
╔╔ 
(
╔╔ 
)
╔╔ 
;
╔╔ 
app╦╦ 
.
╦╦ 
UseAuthentication
╦╦ 
(
╦╦ 
)
╦╦ 
;
╦╦ 
app╠╠ 
.
╠╠ 
UseAuthorization
╠╠ 
(
╠╠ 
)
╠╠ 
;
╠╠ 
app╬╬ 
.
╬╬ 
MapControllers
╬╬ 
(
╬╬ 
)
╬╬ 
;
╬╬ 
await╨╨ 
app
╨╨ 	
.
╨╨	 

RunAsync
╨╨
 
(
╨╨ 
)
╨╨ 
;
╨╨ c
aC:\Users\rajat\Downloads\Rishabh Soft\Project.NET\VoyageHub-Backend\VoyageHub.Api\GlobalUsings.csї|
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
$strYY )
)YY) *
]YY* +
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

employeeId	ZZ| Ж
)
ZZЖ З
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
АА 
Ok
АА 
(
АА 
result
АА 
.
АА 
Segment
АА  
)
АА  !
;
АА! "
}
ББ 
[
ГГ 
HttpPost
ГГ 
(
ГГ 
$str
ГГ 
)
ГГ 
]
ГГ 
[
ДД 
	Authorize
ДД 
(
ДД 
Roles
ДД 
=
ДД 
$str
ДД /
)
ДД/ 0
]
ДД0 1
public
ЕЕ 

async
ЕЕ 
Task
ЕЕ 
<
ЕЕ 
IActionResult
ЕЕ #
>
ЕЕ# $
AddStay
ЕЕ% ,
(
ЕЕ, -
Guid
ЕЕ- 1
id
ЕЕ2 4
,
ЕЕ4 5
[
ЕЕ6 7
FromBody
ЕЕ7 ?
]
ЕЕ? @

AddStayDto
ЕЕA K
dto
ЕЕL O
)
ЕЕO P
{
ЖЖ 
var
ЗЗ 
result
ЗЗ 
=
ЗЗ 
await
ЗЗ #
_travelRequestService
ЗЗ 0
.
ЗЗ0 1
AddStayAsync
ЗЗ1 =
(
ЗЗ= >
id
ЗЗ> @
,
ЗЗ@ A
dto
ЗЗB E
)
ЗЗE F
;
ЗЗF G
if
ИИ 

(
ИИ 
!
ИИ 
result
ИИ 
.
ИИ 
Success
ИИ 
)
ИИ 
return
ИИ #
NotFound
ИИ$ ,
(
ИИ, -
new
ИИ- 0
{
ИИ1 2
message
ИИ3 :
=
ИИ; <
result
ИИ= C
.
ИИC D
Message
ИИD K
}
ИИL M
)
ИИM N
;
ИИN O
return
ЙЙ 
Ok
ЙЙ 
(
ЙЙ 
result
ЙЙ 
.
ЙЙ 
Stay
ЙЙ 
)
ЙЙ 
;
ЙЙ 
}
КК 
[
ММ 
HttpPost
ММ 
(
ММ 
$str
ММ 
)
ММ 
]
ММ 
[
НН 
	Authorize
НН 
(
НН 
Roles
НН 
=
НН 
$str
НН 7
)
НН7 8
]
НН8 9
public
ОО 

async
ОО 
Task
ОО 
<
ОО 
IActionResult
ОО #
>
ОО# $
CloseRequest
ОО% 1
(
ОО1 2
Guid
ОО2 6
id
ОО7 9
)
ОО9 :
{
ПП 
var
РР 
result
РР 
=
РР 
await
РР #
_travelRequestService
РР 0
.
РР0 1
CloseRequestAsync
РР1 B
(
РРB C
id
РРC E
)
РРE F
;
РРF G
if
СС 

(
СС 
!
СС 
result
СС 
.
СС 
Success
СС 
)
СС 
return
СС #
NotFound
СС$ ,
(
СС, -
new
СС- 0
{
СС1 2
message
СС3 :
=
СС; <
result
СС= C
.
ССC D
Message
ССD K
}
ССL M
)
ССM N
;
ССN O
return
ТТ 
Ok
ТТ 
(
ТТ 
new
ТТ 
{
ТТ 
success
ТТ 
=
ТТ  !
true
ТТ" &
,
ТТ& '
message
ТТ( /
=
ТТ0 1
result
ТТ2 8
.
ТТ8 9
Message
ТТ9 @
}
ТТA B
)
ТТB C
;
ТТC D
}
УУ 
}ФФ ▒L
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
cancellationToken	FF| Н
)
FFН О
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
cancellationToken	[[y К
)
[[К Л
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
}mm ╪?
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
HttpGet   
(   
$str   
)   
]   
[!! 
	Authorize!! 
(!! 
Roles!! 
=!! 
$str!! 7
)!!7 8
]!!8 9
public"" 

async"" 
Task"" 
<"" 
ActionResult"" "
<""" #
PolicyConfigDto""# 2
>""2 3
>""3 4
GetMyPolicy""5 @
(""@ A
CancellationToken""A R
cancellationToken""S d
)""d e
{## 
var$$ 
policy$$ 
=$$ 
await$$ 
_policyService$$ )
.$$) *
GetMyPolicyAsync$$* :
($$: ;
cancellationToken$$; L
)$$L M
;$$M N
if%% 

(%% 
policy%% 
==%% 
null%% 
)%% 
{&& 	
return'' 
NotFound'' 
('' 
new'' 
{''  !
message''" )
=''* +
$str'', ^
}''_ `
)''` a
;''a b
}(( 	
return** 
Ok** 
(** 
policy** 
)** 
;** 
}++ 
[-- 
HttpGet-- 
(-- 
$str-- 
)-- 
]-- 
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
<//" #
List//# '
<//' (
string//( .
>//. /
>/// 0
>//0 1#
GetAvailableDepartments//2 I
(//I J
CancellationToken//J [
cancellationToken//\ m
)//m n
{00 
var11 
depts11 
=11 
await11 
_policyService11 (
.11( )(
GetAvailableDepartmentsAsync11) E
(11E F
cancellationToken11F W
)11W X
;11X Y
return22 
Ok22 
(22 
depts22 
)22 
;22 
}33 
[55 
HttpPut55 
(55 
$str55 
)55 
]55 
[66 
	Authorize66 
(66 
Roles66 
=66 
$str66 
)66 
]66  
public77 

async77 
Task77 
<77 
IActionResult77 #
>77# $
UpdatePolicy77% 1
(771 2
Guid772 6
id777 9
,779 :
[77; <
FromBody77< D
]77D E
UpdatePolicyDto77F U
dto77V Y
,77Y Z
CancellationToken77[ l
cancellationToken77m ~
)77~ 
{88 
var99 
(99 
success99 
,99 
message99 
)99 
=99  
await99! &
_policyService99' 5
.995 6
UpdatePolicyAsync996 G
(99G H
id99H J
,99J K
dto99L O
,99O P
cancellationToken99Q b
)99b c
;99c d
if:: 

(:: 
!:: 
success:: 
):: 
{;; 	
return<< 
NotFound<< 
(<< 
new<< 
{<<  !
message<<" )
}<<* +
)<<+ ,
;<<, -
}== 	
return?? 
Ok?? 
(?? 
new?? 
{?? 
success?? 
=??  !
true??" &
,??& '
message??( /
}??0 1
)??1 2
;??2 3
}@@ 
[BB 
HttpPostBB 
]BB 
[CC 
	AuthorizeCC 
(CC 
RolesCC 
=CC 
$strCC 
)CC 
]CC  
publicDD 

asyncDD 
TaskDD 
<DD 
ActionResultDD "
<DD" #
PolicyConfigDtoDD# 2
>DD2 3
>DD3 4
CreatePolicyDD5 A
(DDA B
[DDB C
FromBodyDDC K
]DDK L
CreatePolicyDtoDDM \
dtoDD] `
,DD` a
CancellationTokenDDb s
cancellationToken	DDt Е
)
DDЕ Ж
{EE 
varFF 
(FF 
successFF 
,FF 
messageFF 
,FF 
policyFF %
)FF% &
=FF' (
awaitFF) .
_policyServiceFF/ =
.FF= >
CreatePolicyAsyncFF> O
(FFO P
dtoFFP S
,FFS T
cancellationTokenFFU f
)FFf g
;FFg h
ifGG 

(GG 
!GG 
successGG 
)GG 
{HH 	
returnII 

BadRequestII 
(II 
newII !
{II" #
messageII$ +
}II, -
)II- .
;II. /
}JJ 	
returnLL 
OkLL 
(LL 
policyLL 
)LL 
;LL 
}MM 
[OO 

HttpDeleteOO 
(OO 
$strOO 
)OO 
]OO 
[PP 
	AuthorizePP 
(PP 
RolesPP 
=PP 
$strPP 
)PP 
]PP  
publicQQ 

asyncQQ 
TaskQQ 
<QQ 
IActionResultQQ #
>QQ# $
DeletePolicyQQ% 1
(QQ1 2
GuidQQ2 6
idQQ7 9
,QQ9 :
CancellationTokenQQ; L
cancellationTokenQQM ^
)QQ^ _
{RR 
varSS 
(SS 
successSS 
,SS 
messageSS 
)SS 
=SS  
awaitSS! &
_policyServiceSS' 5
.SS5 6
DeletePolicyAsyncSS6 G
(SSG H
idSSH J
,SSJ K
cancellationTokenSSL ]
)SS] ^
;SS^ _
ifTT 

(TT 
!TT 
successTT 
)TT 
{UU 	
returnVV 
NotFoundVV 
(VV 
newVV 
{VV  !
messageVV" )
}VV* +
)VV+ ,
;VV, -
}WW 	
returnYY 
OkYY 
(YY 
newYY 
{YY 
successYY 
=YY  !
trueYY" &
,YY& '
messageYY( /
}YY0 1
)YY1 2
;YY2 3
}ZZ 
}[[ пн
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
[(( 	
	FromQuery((	 
](( 
string(( 
?(( 
status(( "
=((# $
$str((% .
,((. /
CancellationToken)) 
cancellationToken)) +
=)), -
default)). 5
)))5 6
{** 
var++ 
result++ 
=++ 
string++ 
.++ 
IsNullOrWhiteSpace++ .
(++. /
status++/ 5
)++5 6
||++7 9
string++: @
.++@ A
Equals++A G
(++G H
status++H N
,++N O
$str++P Y
,++Y Z
StringComparison++[ k
.++k l
OrdinalIgnoreCase++l }
)++} ~
?,, 
await,, 
_expenseService,, #
.,,# $"
GetPendingSummaryAsync,,$ :
(,,: ;
category,,; C
,,,C D
highPriorityOnly,,E U
,,,U V
cancellationToken,,W h
),,h i
:-- 
await-- 
_expenseService-- #
.--# $#
GetExpensesSummaryAsync--$ ;
(--; <
status--< B
,--B C
category--D L
,--L M
highPriorityOnly--N ^
,--^ _
cancellationToken--` q
)--q r
;--r s
return.. 
Ok.. 
(.. 
result.. 
).. 
;.. 
}// 
[11 
HttpGet11 
(11 
$str11 
)11 
]11 
[22 
	Authorize22 
(22 
Roles22 
=22 
$str22 7
)227 8
]228 9
public33 

async33 
Task33 
<33 
ActionResult33 "
<33" #
List33# '
<33' (
ExpenseItemDto33( 6
>336 7
>337 8
>338 9
GetMyExpenses33: G
(33G H
CancellationToken33H Y
cancellationToken33Z k
)33k l
{44 
var55 
result55 
=55 
await55 
_expenseService55 *
.55* +
GetMyExpensesAsync55+ =
(55= >
cancellationToken55> O
)55O P
;55P Q
return66 
Ok66 
(66 
result66 
)66 
;66 
}77 
[99 
HttpPost99 
]99 
[:: 
	Authorize:: 
(:: 
Roles:: 
=:: 
$str:: 7
)::7 8
]::8 9
public;; 

async;; 
Task;; 
<;; 
IActionResult;; #
>;;# $
SubmitExpense;;% 2
(;;2 3
[;;3 4
FromBody;;4 <
];;< =
SubmitExpenseDto;;> N
dto;;O R
,;;R S
CancellationToken;;T e
cancellationToken;;f w
);;w x
{<< 
var== 
(== 
success== 
,== 
message== 
,== 
expense== &
)==& '
===( )
await==* /
_expenseService==0 ?
.==? @
SubmitExpenseAsync==@ R
(==R S
dto==S V
,==V W
cancellationToken==X i
)==i j
;==j k
if>> 

(>> 
!>> 
success>> 
)>> 
{?? 	
return@@ 

BadRequest@@ 
(@@ 
new@@ !
{@@" #
message@@$ +
}@@, -
)@@- .
;@@. /
}AA 	
returnCC 
OkCC 
(CC 
expenseCC 
)CC 
;CC 
}DD 
[FF 
HttpPostFF 
(FF 
$strFF 
)FF 
]FF 
[GG 
	AuthorizeGG 
(GG 
RolesGG 
=GG 
$strGG &
)GG& '
]GG' (
publicHH 

asyncHH 
TaskHH 
<HH 
IActionResultHH #
>HH# $

TakeActionHH% /
(HH/ 0
GuidHH0 4
idHH5 7
,HH7 8
[HH9 :
FromBodyHH: B
]HHB C
ExpenseActionDtoHHD T
dtoHHU X
,HHX Y
CancellationTokenHHZ k
cancellationTokenHHl }
)HH} ~
{II 
varJJ 
(JJ 
successJJ 
,JJ 
messageJJ 
)JJ 
=JJ  
awaitJJ! &
_expenseServiceJJ' 6
.JJ6 7
TakeActionAsyncJJ7 F
(JJF G
idJJG I
,JJI J
dtoJJK N
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
returnMM 
NotFoundMM 
(MM 
newMM 
{MM  !
messageMM" )
}MM* +
)MM+ ,
;MM, -
}NN 	
returnPP 
OkPP 
(PP 
newPP 
{PP 
successPP 
=PP  !
truePP" &
,PP& '
messagePP( /
}PP0 1
)PP1 2
;PP2 3
}QQ 
[SS 
HttpPostSS 
(SS 
$strSS 
)SS 
]SS 
[TT 
	AuthorizeTT 
(TT 
RolesTT 
=TT 
$strTT &
)TT& '
]TT' (
publicUU 

asyncUU 
TaskUU 
<UU 
IActionResultUU #
>UU# $
BatchPayoutUU% 0
(UU0 1
[UU1 2
FromBodyUU2 :
]UU: ;
BatchPayoutDtoUU< J
?UUJ K
dtoUUL O
,UUO P
CancellationTokenUUQ b
cancellationTokenUUc t
)UUt u
{VV 
varWW 
(WW 
successWW 
,WW 
messageWW 
)WW 
=WW  
awaitWW! &
_expenseServiceWW' 6
.WW6 7
BatchPayoutAsyncWW7 G
(WWG H
dtoWWH K
,WWK L
cancellationTokenWWM ^
)WW^ _
;WW_ `
ifXX 

(XX 
!XX 
successXX 
)XX 
{YY 	
returnZZ 

BadRequestZZ 
(ZZ 
newZZ !
{ZZ" #
messageZZ$ +
}ZZ, -
)ZZ- .
;ZZ. /
}[[ 	
return]] 
Ok]] 
(]] 
new]] 
{]] 
success]] 
=]]  !
true]]" &
,]]& '
message]]( /
}]]0 1
)]]1 2
;]]2 3
}^^ 
[`` 
HttpPost`` 
(`` 
$str`` 
)`` 
]`` 
[aa 
	Authorizeaa 
(aa 
Rolesaa 
=aa 
$straa 7
)aa7 8
]aa8 9
publicbb 

asyncbb 
Taskbb 
<bb 
IActionResultbb #
>bb# $
UploadProofbb% 0
(bb0 1
	IFormFilebb1 :
?bb: ;
filebb< @
,bb@ A
CancellationTokenbbB S
cancellationTokenbbT e
)bbe f
{cc 
vardd 
(dd 
errorResultdd 
,dd 
relativeUrldd %
,dd% &
fileNamedd' /
)dd/ 0
=dd1 2
awaitdd3 8
SaveProofFileAsyncdd9 K
(ddK L
fileddL P
,ddP Q
cancellationTokenddR c
)ddc d
;ddd e
ifee 

(ee 
errorResultee 
!=ee 
nullee 
)ee  
{ff 	
returngg 
errorResultgg 
;gg 
}hh 	
returnjj 
Okjj 
(jj 
newjj 
{kk 	
successll 
=ll 
truell 
,ll 
	proofPathmm 
=mm 
relativeUrlmm #
,mm# $
fileNamenn 
,nn 
fileUrloo 
=oo 
$"oo 
{oo 
Requestoo  
.oo  !
Schemeoo! '
}oo' (
$stroo( +
{oo+ ,
Requestoo, 3
.oo3 4
Hostoo4 8
}oo8 9
{oo9 :
relativeUrloo: E
}ooE F
"ooF G
}pp 	
)pp	 

;pp
 
}qq 
[ss 
HttpPostss 
(ss 
$strss 
)ss 
]ss 
[tt 
	Authorizett 
(tt 
Rolestt 
=tt 
$strtt 7
)tt7 8
]tt8 9
publicuu 

asyncuu 
Taskuu 
<uu 
IActionResultuu #
>uu# $
AttachProofuu% 0
(uu0 1
Guiduu1 5
iduu6 8
,uu8 9
	IFormFileuu: C
?uuC D
fileuuE I
,uuI J
CancellationTokenuuK \
cancellationTokenuu] n
)uun o
{vv 
varww 
(ww 
errorResultww 
,ww 
relativeUrlww %
,ww% &
fileNameww' /
)ww/ 0
=ww1 2
awaitww3 8
SaveProofFileAsyncww9 K
(wwK L
filewwL P
,wwP Q
cancellationTokenwwR c
)wwc d
;wwd e
ifxx 

(xx 
errorResultxx 
!=xx 
nullxx 
)xx  
{yy 	
returnzz 
errorResultzz 
;zz 
}{{ 	
var}} 
(}} 
success}} 
,}} 
message}} 
,}} 
	proofPath}} (
)}}( )
=}}* +
await}}, 1
_expenseService}}2 A
.}}A B
AttachProofAsync}}B R
(}}R S
id}}S U
,}}U V
relativeUrl}}W b
!}}b c
,}}c d
cancellationToken}}e v
)}}v w
;}}w x
if~~ 

(~~ 
!~~ 
success~~ 
)~~ 
{ 	
return
АА 
NotFound
АА 
(
АА 
new
АА 
{
АА  !
message
АА" )
}
АА* +
)
АА+ ,
;
АА, -
}
ББ 	
return
ГГ 
Ok
ГГ 
(
ГГ 
new
ГГ 
{
ДД 	
success
ЕЕ 
=
ЕЕ 
true
ЕЕ 
,
ЕЕ 
	proofPath
ЖЖ 
=
ЖЖ 
relativeUrl
ЖЖ #
,
ЖЖ# $
fileName
ЗЗ 
,
ЗЗ 
fileUrl
ИИ 
=
ИИ 
$"
ИИ 
{
ИИ 
Request
ИИ  
.
ИИ  !
Scheme
ИИ! '
}
ИИ' (
$str
ИИ( +
{
ИИ+ ,
Request
ИИ, 3
.
ИИ3 4
Host
ИИ4 8
}
ИИ8 9
{
ИИ9 :
relativeUrl
ИИ: E
}
ИИE F
"
ИИF G
,
ИИG H
message
ЙЙ 
}
КК 	
)
КК	 

;
КК
 
}
ЛЛ 
private
НН 
async
НН 
Task
НН 
<
НН 
(
НН 
IActionResult
НН %
?
НН% &
ErrorResult
НН' 2
,
НН2 3
string
НН4 :
?
НН: ;
RelativeUrl
НН< G
,
ННG H
string
ННI O
?
ННO P
FileName
ННQ Y
)
ННY Z
>
ННZ [ 
SaveProofFileAsync
НН\ n
(
ННn o
	IFormFile
ННo x
?
ННx y
file
ННz ~
,
НН~ !
CancellationTokenННА С!
cancellationTokenННТ г
)ННг д
{
ОО 
if
ПП 

(
ПП 
file
ПП 
==
ПП 
null
ПП 
||
ПП 
file
ПП  
.
ПП  !
Length
ПП! '
==
ПП( *
$num
ПП+ ,
)
ПП, -
{
РР 	
return
СС 
(
СС 

BadRequest
СС 
(
СС 
new
СС "
{
СС# $
message
СС% ,
=
СС- .
$str
СС/ R
}
ССS T
)
ССT U
,
ССU V
null
ССW [
,
СС[ \
null
СС] a
)
ССa b
;
ССb c
}
ТТ 	
if
ФФ 

(
ФФ 
file
ФФ 
.
ФФ 
Length
ФФ 
>
ФФ 
$num
ФФ 
*
ФФ 
$num
ФФ #
*
ФФ$ %
$num
ФФ& *
)
ФФ* +
{
ХХ 	
return
ЦЦ 
(
ЦЦ 

BadRequest
ЦЦ 
(
ЦЦ 
new
ЦЦ "
{
ЦЦ# $
message
ЦЦ% ,
=
ЦЦ- .
$str
ЦЦ/ R
}
ЦЦS T
)
ЦЦT U
,
ЦЦU V
null
ЦЦW [
,
ЦЦ[ \
null
ЦЦ] a
)
ЦЦa b
;
ЦЦb c
}
ЧЧ 	
var
ЩЩ 
ext
ЩЩ 
=
ЩЩ 
Path
ЩЩ 
.
ЩЩ 
GetExtension
ЩЩ #
(
ЩЩ# $
file
ЩЩ$ (
.
ЩЩ( )
FileName
ЩЩ) 1
)
ЩЩ1 2
.
ЩЩ2 3
ToLowerInvariant
ЩЩ3 C
(
ЩЩC D
)
ЩЩD E
;
ЩЩE F
if
ЪЪ 

(
ЪЪ 
!
ЪЪ 
AllowedExtensions
ЪЪ 
.
ЪЪ 
Contains
ЪЪ '
(
ЪЪ' (
ext
ЪЪ( +
)
ЪЪ+ ,
)
ЪЪ, -
{
ЫЫ 	
return
ЬЬ 
(
ЬЬ 

BadRequest
ЬЬ 
(
ЬЬ 
new
ЬЬ "
{
ЬЬ# $
message
ЬЬ% ,
=
ЬЬ- .
$strЬЬ/ А
}ЬЬБ В
)ЬЬВ Г
,ЬЬГ Д
nullЬЬЕ Й
,ЬЬЙ К
nullЬЬЛ П
)ЬЬП Р
;ЬЬР С
}
ЭЭ 	
var
ЯЯ 
receiptsDir
ЯЯ 
=
ЯЯ 
Path
ЯЯ 
.
ЯЯ 
Combine
ЯЯ &
(
ЯЯ& '
_environment
ЯЯ' 3
.
ЯЯ3 4
ContentRootPath
ЯЯ4 C
,
ЯЯC D
$str
ЯЯE O
,
ЯЯO P
$str
ЯЯQ [
)
ЯЯ[ \
;
ЯЯ\ ]
if
аа 

(
аа 
!
аа 
	Directory
аа 
.
аа 
Exists
аа 
(
аа 
receiptsDir
аа )
)
аа) *
)
аа* +
{
бб 	
	Directory
вв 
.
вв 
CreateDirectory
вв %
(
вв% &
receiptsDir
вв& 1
)
вв1 2
;
вв2 3
}
гг 	
var
ее 
safeFileName
ее 
=
ее 
$"
ее 
{
ее 
Guid
ее "
.
ее" #
NewGuid
ее# *
(
ее* +
)
ее+ ,
:
ее, -
$str
ее- .
}
ее. /
$str
ее/ 0
{
ее0 1
Path
ее1 5
.
ее5 6)
GetFileNameWithoutExtension
ее6 Q
(
ееQ R
file
ееR V
.
ееV W
FileName
ееW _
)
ее_ `
.
ее` a
Replace
ееa h
(
ееh i
$str
ееi l
,
ееl m
$str
ееn q
)
ееq r
}
ееr s
{
ееs t
ext
ееt w
}
ееw x
"
ееx y
;
ееy z
var
жж 
filePath
жж 
=
жж 
Path
жж 
.
жж 
Combine
жж #
(
жж# $
receiptsDir
жж$ /
,
жж/ 0
safeFileName
жж1 =
)
жж= >
;
жж> ?
await
ии 
using
ии 
(
ии 
var
ии 
stream
ии 
=
ии  !
new
ии" %

FileStream
ии& 0
(
ии0 1
filePath
ии1 9
,
ии9 :
FileMode
ии; C
.
ииC D
Create
ииD J
)
ииJ K
)
ииK L
{
йй 	
await
кк 
file
кк 
.
кк 
CopyToAsync
кк "
(
кк" #
stream
кк# )
,
кк) *
cancellationToken
кк+ <
)
кк< =
;
кк= >
}
лл 	
var
нн 
relativeUrl
нн 
=
нн 
$"
нн 
$str
нн 0
{
нн0 1
safeFileName
нн1 =
}
нн= >
"
нн> ?
;
нн? @
return
оо 
(
оо 
null
оо 
,
оо 
relativeUrl
оо !
,
оо! "
file
оо# '
.
оо' (
FileName
оо( 0
)
оо0 1
;
оо1 2
}
пп 
[
▒▒ 
HttpGet
▒▒ 
(
▒▒ 
$str
▒▒ 
)
▒▒  
]
▒▒  !
[
▓▓ 
	Authorize
▓▓ 
(
▓▓ 
Roles
▓▓ 
=
▓▓ 
$str
▓▓ 7
)
▓▓7 8
]
▓▓8 9
public
││ 

IActionResult
││ 
GetProofFile
││ %
(
││% &
string
││& ,
fileName
││- 5
)
││5 6
{
┤┤ 
if
╡╡ 

(
╡╡ 
string
╡╡ 
.
╡╡  
IsNullOrWhiteSpace
╡╡ %
(
╡╡% &
fileName
╡╡& .
)
╡╡. /
)
╡╡/ 0
{
╢╢ 	
return
╖╖ 

BadRequest
╖╖ 
(
╖╖ 
new
╖╖ !
{
╖╖" #
message
╖╖$ +
=
╖╖, -
$str
╖╖. F
}
╖╖G H
)
╖╖H I
;
╖╖I J
}
╕╕ 	
var
║║ 
	cleanName
║║ 
=
║║ 
Path
║║ 
.
║║ 
GetFileName
║║ (
(
║║( )
fileName
║║) 1
)
║║1 2
;
║║2 3
var
╗╗ 
receiptsDir
╗╗ 
=
╗╗ 
Path
╗╗ 
.
╗╗ 
Combine
╗╗ &
(
╗╗& '
_environment
╗╗' 3
.
╗╗3 4
ContentRootPath
╗╗4 C
,
╗╗C D
$str
╗╗E O
,
╗╗O P
$str
╗╗Q [
)
╗╗[ \
;
╗╗\ ]
var
╝╝ 
filePath
╝╝ 
=
╝╝ 
Path
╝╝ 
.
╝╝ 
Combine
╝╝ #
(
╝╝# $
receiptsDir
╝╝$ /
,
╝╝/ 0
	cleanName
╝╝1 :
)
╝╝: ;
;
╝╝; <
if
╛╛ 

(
╛╛ 
!
╛╛ 
System
╛╛ 
.
╛╛ 
IO
╛╛ 
.
╛╛ 
File
╛╛ 
.
╛╛ 
Exists
╛╛ "
(
╛╛" #
filePath
╛╛# +
)
╛╛+ ,
)
╛╛, -
{
┐┐ 	
return
└└ 
NotFound
└└ 
(
└└ 
new
└└ 
{
└└  !
message
└└" )
=
└└* +
$str
└└, I
}
└└J K
)
└└K L
;
└└L M
}
┴┴ 	
var
├├ 
ext
├├ 
=
├├ 
Path
├├ 
.
├├ 
GetExtension
├├ #
(
├├# $
	cleanName
├├$ -
)
├├- .
.
├├. /
ToLowerInvariant
├├/ ?
(
├├? @
)
├├@ A
;
├├A B
var
── 
contentType
── 
=
── 
ext
── 
switch
── $
{
┼┼ 	
$str
╞╞ 
=>
╞╞ 
$str
╞╞ '
,
╞╞' (
$str
╟╟ 
=>
╟╟ 
$str
╟╟ !
,
╟╟! "
$str
╚╚ 
or
╚╚ 
$str
╚╚ 
=>
╚╚  
$str
╚╚! -
,
╚╚- .
$str
╔╔ 
=>
╔╔ 
$str
╔╔ #
,
╔╔# $
_
╩╩ 
=>
╩╩ 
$str
╩╩ +
}
╦╦ 	
;
╦╦	 

var
══ 
stream
══ 
=
══ 
new
══ 

FileStream
══ #
(
══# $
filePath
══$ ,
,
══, -
FileMode
══. 6
.
══6 7
Open
══7 ;
,
══; <

FileAccess
══= G
.
══G H
Read
══H L
,
══L M
	FileShare
══N W
.
══W X
Read
══X \
)
══\ ]
;
══] ^
return
╬╬ 
File
╬╬ 
(
╬╬ 
stream
╬╬ 
,
╬╬ 
contentType
╬╬ '
,
╬╬' (
	cleanName
╬╬) 2
,
╬╬2 3#
enableRangeProcessing
╬╬4 I
:
╬╬I J
true
╬╬K O
)
╬╬O P
;
╬╬P Q
}
╧╧ 
}╨╨ ╠U
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
cancellationToken	22t Е
)
22Е Ж
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
cancellationToken	==} О
)
==О П
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
varUU 

uploadFileUU 
=UU 
fileUU 
;UU 
ifWW 

(WW 

uploadFileWW 
==WW 
nullWW 
||WW !

uploadFileWW" ,
.WW, -
LengthWW- 3
==WW4 6
$numWW7 8
)WW8 9
{XX 	
returnYY 

BadRequestYY 
(YY 
newYY !
{YY" #
messageYY$ +
=YY, -
$strYY. P
}YYQ R
)YYR S
;YYS T
}ZZ 	
if\\ 

(\\ 
!\\ 

uploadFile\\ 
.\\ 
FileName\\  
.\\  !
EndsWith\\! )
(\\) *
$str\\* 0
,\\0 1
StringComparison\\2 B
.\\B C
OrdinalIgnoreCase\\C T
)\\T U
)\\U V
{]] 	
return^^ 

BadRequest^^ 
(^^ 
new^^ !
{^^" #
message^^$ +
=^^, -
$str^^. _
}^^` a
)^^a b
;^^b c
}__ 	
usingaa 
varaa 
streamaa 
=aa 

uploadFileaa %
.aa% &
OpenReadStreamaa& 4
(aa4 5
)aa5 6
;aa6 7
varbb 
resultbb 
=bb 
awaitbb 
_employeeServicebb +
.bb+ ,
BatchUploadCsvAsyncbb, ?
(bb? @
streambb@ F
,bbF G
cancellationTokenbbH Y
)bbY Z
;bbZ [
returncc 
Okcc 
(cc 
resultcc 
)cc 
;cc 
}dd 
[ff 
HttpGetff 
(ff 
$strff 
)ff 
]ff 
[gg 
AllowAnonymousgg 
]gg 
publichh 

IActionResulthh 
DownloadCsvTemplatehh ,
(hh, -
)hh- .
{ii 
varjj 

csvBuilderjj 
=jj 
newjj 
StringBuilderjj *
(jj* +
)jj+ ,
;jj, -

csvBuilderkk 
.kk 

AppendLinekk 
(kk 
$strkk C
)kkC D
;kkD E

csvBuilderll 
.ll 

AppendLinell 
(ll 
$strll h
)llh i
;lli j

csvBuildermm 
.mm 

AppendLinemm 
(mm 
$strmm l
)mml m
;mmm n

csvBuildernn 
.nn 

AppendLinenn 
(nn 
$strnn p
)nnp q
;nnq r
varpp 
bytespp 
=pp 
Encodingpp 
.pp 
UTF8pp !
.pp! "
GetBytespp" *
(pp* +

csvBuilderpp+ 5
.pp5 6
ToStringpp6 >
(pp> ?
)pp? @
)pp@ A
;ppA B
returnqq 
Fileqq 
(qq 
bytesqq 
,qq 
$strqq %
,qq% &
$strqq' ?
)qq? @
;qq@ A
}rr 
}ss Цb
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
cancellationToken	CCv З
)
CCЗ И
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
cancellationToken	WWx Й
)
WWЙ К
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
CancellationToken	kkp Б
cancellationToken
kkВ У
)
kkУ Ф
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
АА 	
}
ББ 
[
ГГ 
HttpPost
ГГ 
(
ГГ 
$str
ГГ "
)
ГГ" #
]
ГГ# $
[
ДД 
AllowAnonymous
ДД 
]
ДД 
public
ЕЕ 

async
ЕЕ 
Task
ЕЕ 
<
ЕЕ 
ActionResult
ЕЕ "
<
ЕЕ" #
AuthResponseDto
ЕЕ# 2
>
ЕЕ2 3
>
ЕЕ3 4
VerifyResetToken
ЕЕ5 E
(
ЕЕE F
[
ЕЕF G
FromBody
ЕЕG O
]
ЕЕO P!
VerifyResetTokenDto
ЕЕQ d
dto
ЕЕe h
,
ЕЕh i
CancellationToken
ЕЕj { 
cancellationTokenЕЕ| Н
)ЕЕН О
{
ЖЖ 
var
ЗЗ 
(
ЗЗ 

statusCode
ЗЗ 
,
ЗЗ 
response
ЗЗ !
)
ЗЗ! "
=
ЗЗ# $
await
ЗЗ% *
_authService
ЗЗ+ 7
.
ЗЗ7 8#
VerifyResetTokenAsync
ЗЗ8 M
(
ЗЗM N
dto
ЗЗN Q
,
ЗЗQ R
cancellationToken
ЗЗS d
)
ЗЗd e
;
ЗЗe f
return
ИИ 

StatusCode
ИИ 
(
ИИ 

statusCode
ИИ $
,
ИИ$ %
response
ИИ& .
)
ИИ. /
;
ИИ/ 0
}
ЙЙ 
[
ЛЛ 
HttpPost
ЛЛ 
(
ЛЛ 
$str
ЛЛ 
)
ЛЛ 
]
ЛЛ  
[
ММ 
AllowAnonymous
ММ 
]
ММ 
public
НН 

async
НН 
Task
НН 
<
НН 
ActionResult
НН "
<
НН" #
AuthResponseDto
НН# 2
>
НН2 3
>
НН3 4
ResetPassword
НН5 B
(
ННB C
[
ННC D
FromBody
ННD L
]
ННL M
ResetPasswordDto
ННN ^
dto
НН_ b
,
ННb c
CancellationToken
ННd u 
cancellationTokenННv З
)ННЗ И
{
ОО 
var
ПП 
(
ПП 

statusCode
ПП 
,
ПП 
response
ПП !
)
ПП! "
=
ПП# $
await
ПП% *
_authService
ПП+ 7
.
ПП7 8 
ResetPasswordAsync
ПП8 J
(
ППJ K
dto
ППK N
,
ППN O
cancellationToken
ППP a
)
ППa b
;
ППb c
return
РР 

StatusCode
РР 
(
РР 

statusCode
РР $
,
РР$ %
response
РР& .
)
РР. /
;
РР/ 0
}
СС 
}ТТ ё
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
}!! ╖
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
}++ Ь
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
cancellationToken	""q В
)
""В Г
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
},, 