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
<ADDINS/>
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

## 更新日志 ##

#### 2021-11-08

### 版本1.0.34

- 当操作系统没有默认浏览器时，点击管理报错处理。
- 

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
