module MxHttp.MediaTypeTests

open Xunit

[<Fact>]
let ``given applicationJson when toString then returns correct media type`` () =
    let actual = MediaType.applicationJson

    Assert.Equal("application/json", MediaType.toString actual)

[<Fact>]
let ``given applicationXml when toString then returns correct media type`` () =
    let actual = MediaType.applicationXml

    Assert.Equal("application/xml", MediaType.toString actual)

[<Fact>]
let ``given applicationOctetStream when toString then returns correct media type`` () =
    let actual = MediaType.applicationOctetStream

    Assert.Equal("application/octet-stream", MediaType.toString actual)

[<Fact>]
let ``given applicationFormUrlEncoded when toString then returns correct media type`` () =
    let actual = MediaType.applicationFormUrlEncoded

    Assert.Equal("application/x-www-form-urlencoded", MediaType.toString actual)

[<Fact>]
let ``given string when ofString then toString returns correct media type`` () =
    let mediaType = "text/plain"

    let actual = MediaType.ofString mediaType

    Assert.Equal(mediaType, MediaType.toString actual)

[<Fact>]
let ``given same MediaTypes when equals then returns true`` () =
    Assert.True(MediaType.equals MediaType.applicationFormUrlEncoded MediaType.applicationFormUrlEncoded)

[<Fact>]
let ``given same MediaTypes but in different case when equals then returns true`` () =
    let differentCaseMediaType = MediaType.ofString "Application/XML"
    Assert.True(MediaType.equals differentCaseMediaType MediaType.applicationXml)

[<Fact>]
let ``given different MediaTypes when equals then returns false`` () =
    let differentMediaType = MediaType.ofString "application/xml " // Notice a space.
    Assert.False(MediaType.equals differentMediaType MediaType.applicationXml)
