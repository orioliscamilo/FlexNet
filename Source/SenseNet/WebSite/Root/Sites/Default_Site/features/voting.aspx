﻿<%@ Page Language="C#" CompilationMode="Never" MasterPageFile="~/Root/Global/pagetemplates/sn-layout-inter.Master" %><asp:Content ID="Content_FullCol" ContentPlaceHolderID="CPFullCol" runat="server"><asp:WebPartZone ID="FullCol" name="FullCol" HeaderText="Full Column" PartChromeType="None"  runat="server"><ZoneTemplate><snpe:breadcrumbportlet breadcrumbcssclass="sn-breadcrumb" mode="Linear" separator=" / " id="BreadCrumb1" runat="server" chrometype="None" ShowSite="True" SiteDisplayName="Home" /></ZoneTemplate></asp:WebPartZone></asp:Content><asp:Content ID="Content_LeftCol" ContentPlaceHolderID="CPLeftCol" runat="server"><asp:WebPartZone ID="LeftCol" name="LeftCol" HeaderText="Left Column" PartChromeType="TitleAndBorder"  runat="server"><ZoneTemplate><snpe:SiteMenu Title="Features" BindTarget="CurrentWorkspace" ShowPagesOnly="false" GetContextChildren="true" OmitContextNode="true" ExpandToContext="true" Depth="1" ID="MainMenu" runat="server" /><snpe:AdvancedLoginPortlet Title="Profile" ID="login" runat="server" LoginViewPath="/Root/System/SystemPlugins/Portlets/AdvancedLogin/LoginView.ascx" /></ZoneTemplate></asp:WebPartZone></asp:Content><asp:Content ID="Content_WideCol" ContentPlaceHolderID="CPWideCol" runat="server"><asp:WebPartZone ID="WideCol" name="WideCol" HeaderText="Wide Column" PartChromeType="TitleAndBorder"  runat="server"><ZoneTemplate></ZoneTemplate></asp:WebPartZone></asp:Content><asp:Content ID="Content_CenterCol" ContentPlaceHolderID="CPCenterCol" runat="server"><asp:WebPartZone ID="CenterCol" name="CenterCol" HeaderText="Center Column" PartChromeType="TitleAndBorder"  runat="server"><ZoneTemplate></ZoneTemplate></asp:WebPartZone></asp:Content><asp:Content ID="Content_CenterLeftCol" ContentPlaceHolderID="CPCenterLeftCol" runat="server"><asp:WebPartZone ID="CenterLeftCol" name="CenterLeftCol" HeaderText="Center / Left Column" PartChromeType="TitleAndBorder"  runat="server"><ZoneTemplate></ZoneTemplate></asp:WebPartZone></asp:Content><asp:Content ID="Content_CenterRightCol" ContentPlaceHolderID="CPCenterRightCol" runat="server"><asp:WebPartZone ID="CenterRightCol" name="CenterRightCol" HeaderText="Center / Right Column" PartChromeType="TitleAndBorder"  runat="server"><ZoneTemplate></ZoneTemplate></asp:WebPartZone></asp:Content><asp:Content ID="Content_RightCol" ContentPlaceHolderID="CPRightCol" runat="server"><asp:WebPartZone ID="RightCol" name="RightCol" HeaderText="Right Column" PartChromeType="TitleAndBorder"  runat="server"><ZoneTemplate></ZoneTemplate></asp:WebPartZone></asp:Content><asp:Content ID="Content_Footer" ContentPlaceHolderID="CPFooter" runat="server"><asp:WebPartZone ID="Footer" name="Footer" HeaderText="Footer" PartChromeType="None"  runat="server"><ZoneTemplate><snpe:SingleContentPortlet UsedContentTypeName="HTMLContent" ContentPath="/Root/YourContents/FooterContent" ID="FooterContent1" runat="server" /></ZoneTemplate></asp:WebPartZone></asp:Content>