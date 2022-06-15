# WebsysServer #
Chrome调用动态库中间件，提供HTTP服务接口来调用本地服务，提供跨浏览器通过js调用本地服务功能。
操作系统环境Win10, Chrome版本76.0.3809.132。关闭Win10的UAC体验更佳
依赖开发环境：
1. 开发环境Framework4.0
2. 分三个工程，分别为服务功能，保护功能，安装功能
# 使用介绍 #
## 引用方式 ##
### 1. `csp`中引用中间件环境 ###
```html
<ADDINS></ADDINS>
```
### 2.`组件`中引用中间件环境
```vb
d ##class(websys.AddInsTmpl).WriteInvokerJsCode()
```
已配置对象列表如下：

|对象名|方法列表|功能说明|
|:-----:|:--------:|:---------:|
|CmdShell|GetInfo,GetIP,Run|调用cmd命令|
|DHCOPPrint|ToPrintHDLPStr|DHCOPPrint.CAB包打印|
|LODOP|FORMAT,PRINT_INIT,PRINT等方法|LODOP打印对象|
|TrakWebEdit3|ShowLayout|调用组件|
|PrjSetTime|SetTime|设置本地时间|

### 常见问题处理

- 调用对象都有notReturn属性，`DoctorSheet.notReturn=0`即有返回值调用，同步调用。默认为1异步调用

- 调用客户端方法报错，检查桌面快捷方法-插件管理-右键属性-兼容性以管理员身份运行此程序是否勾选

- 在只安装了WPS的客户端，使用Excel导出或打印时报错，可以把`CmdShell.EvalJs(mycode)`修改成`CmdShell.CurrentUserEvalJs(mycode)`再测试

- 安装成功后,HTTP管理界面可用但HTTPS管理界面不可用，可手动安装证书
  1. private.pfx安装---本地计算机---到【个人】中，密码为12345678
  2. private.crt安装---本地计算机---到【受信任的根证书颁发机构】中
  3. netsh http add sslcert ipport=0.0.0.0:21996 certhash=dd8652db5c07076d154827273642604ca8405332 appid={9e977cef-28ef-4d4f-968a-bff2514384c4}
  4. netsh http add sslcert ipport=0.0.0.0:21996 certhash=b1eb8df9b91cf3080fb30f41e959def25952376a appid={9e977cef-28ef-4d4f-968a-bff2514384c4}

## 更新日志 ##

### 2022-06-7

### 版本1.1.0

- 对动态库的线程调用修改成进程调用
- - 调用完成释放资源（2565081 ）
  - 减少动态库间冲突（2683935）

### 2022-04-28
### 版本1.0.39
- 优化返回值的eacape速度 :bug: 
- - 当返回字符长度100kb时，eacape速度要花13秒问题处理


#### 2022-04-24
### 版本1.0.38
- 使用CurrentUserEvalJs调用代码未完成时，又发出CurrentUserEvalJs命令报错处理，修改成多个代码文件处理。

  ```MyCode.txt正由另一进程使用，因此该进程无法访问此文件```

- 使用CurrentUserEvalJs得不到真实返回值，报错会在\temp\目录下对应代码文件中显示

- 使用CurrentUserEvalJs运行脚本时，默认5分钟，超过5分钟提示。

#### 2022-03-03

### 版本1.0.37

- 支持动态升级插件模块 :sparkles:
- 多屏下才启用快捷键[Ctrl+`]定位鼠标

#### 2022-01-18

### 版本1.0.36

- 使用HTTP协议下载Linux服务器上dll，当dll不存在时，不会像window服务器那样报错，而是返回0KB大小内容，导致检验报告打印DLL为0KB大小 :bug:
- - 修改成如果返回0KB大小，则不重写DLL

#### 2021-11-16

### 版本1.0.35

- 使用CurrentUserEvalJs调用代码未完成时，又发出CurrentUserEvalJs命令报错处理

  ```MyCode.txt正由另一进程使用，因此该进程无法访问此文件```

#### 2021-11-08

### 版本1.0.34

- 当操作系统没有默认浏览器时，点击管理报错处理。
- 增加定位鼠标功能配置`CursorShowHotKey`
- 增加对TLS1.2的支持（测试未通过） :sparkles:
- 增加按客户端IP升级 :sparkles:

#### 2021-09-07

#### 版本1.0.33

- 增加[Ctrl+`]定位鼠标

```xml
<setting name="CursorShowHotKey" serializeAs="String">
    <value>192</value> <!-- 配置热键[Ctrl+`]定位光标，192为`的键盘代码 -->
</setting>
```

- 增加异步focus窗口功能，可以解决线程弹出窗口不置顶问题
- - 增加*focusWindowName*，*focusClassName*，*focusLazyTime*三个配置项解决focus窗口问题

调用示例代码：

```js
trakWebEdit3.clear(); /*清除上次调用数据*/
trakWebEdit3.notReturn = 1;
// trakWebEdit3.focusLazyTime = 1000;  /*延迟多长时间focus窗口。没有此行代码时默认:1000毫秒*/
// trakWebEdit3.focusWindowName = "lpWindowName";  /*使用窗口标题定位*/
trakWebEdit3.focusClassName = "lpClassName";  /*使用类名定位，例：微信窗口WeChatMainWndForPC*/
trakWebEdit3.ShowLayout("1^1^^1","54429","","cn_iptcp:127.0.0.1[1972]:DHC-APP",function(rtn){});
/*注：ShowLayout方法配置时不勾【调用清除】*/
```

#### 2021-06-22

#### 版本1.0.32

- 提供获得扩展屏方法 :sparkles:
- 提供移动窗口方法 :sparkles:
- 去除"保护程序已经在运行"提示，去除"监听程序已经在运行"提示（河北中石油中心）

#### 2021-04-24

#### 版本1.0.31

- 默认安装exe,报无效安装程序问题。修改成使用cmd运行
- 关于修改
- 修改说明css
- 优化升级步骤

#### 2021-04-09

#### 版本1.0.30

+ 为解决某些电脑调用WPS时报Automation不能创建错误，提供CurrentUserEvalJs方法（九江中医，北京积水潭）

#### 2021-04-07

#### 版本1.0.29

+ CmdShell.EvalJs降低安全且设定在当前窗口弹出的界面  :sparkles:

#### 2021-02-21

#### 版本1.0.0.28

+ 增加mime管理 :sparkles:
+ 支持静态文件浏览 :sparkles:
+ 响应状态码支持`404`,`500` :bug:

#### 2021-02-07

#### 版本1.0.0.27

+ 中间件保护程序与医保客户端冲突问题处理

### 2019-06-16 ###
#### 版本1.0.0.0 ####
* http协议下POST请求本地服务
* 自动下载配置的文件
* 自动安装exe,msi
* 自动注册ocx,dll
* 反射调用本地程序及cmd
