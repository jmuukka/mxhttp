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
