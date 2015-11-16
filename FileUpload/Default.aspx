<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

	<div class="jumbotron">
		<asp:Label runat="server" id="UploadCompletedMessage" Text="" />
	    <div>
		    <asp:Label runat="server" id="KeywordsLabel" Text="Keywords:" />
		    <asp:TextBox runat="server" id="Keywords" />
	    </div>
	    <asp:FileUpload runat="server" id="FileUploadControl"/>
	    <asp:Button runat="server" id="UploadButton" Text="Upload" OnClick="UploadButton_Click" />

	</div>
</asp:Content>
