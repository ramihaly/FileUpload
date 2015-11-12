<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

	<div class="jumbotron">
		<h1>DAM File Upload</h1>
		<div>
			<asp:Label runat="server" id="KeywordsLabel" Text="Enter some semi-colon (;) separated keywords:" />
			<asp:TextBox runat="server" id="Keywords" />
		</div>
		<asp:FileUpload runat="server" id="FileUploadControl"/>
		<asp:Button runat="server" id="UploadButton" Text="Upload" OnClick="UploadButton_Click" />
	</div>
</asp:Content>
