import React, { useContext } from "react"
import { PostContext } from "../providers/PostProvider"
import { Button } from "reactstrap";

export const PostSearch = () => {
  const { searchPost, setSearchTerm, searchTerm, getAllPosts } = useContext(PostContext)

  return (
    <div className="container">
      <div className="row">
        <div className="col-lg-2 col-sm-2">
          <h5>Search:</h5>
        </div>
        <div className="col-lg-6 col-sm-6">
          <input type="text"
            className="input--wide"
            onKeyUp={(event) => setSearchTerm(event.target.value)}
            placeholder="Search for a post... " />
        </div>
        <div className="col-lg-4 col-sm-4">
          <Button className="btn btn-sm btn-success" onClick={() => searchPost()}>Search</Button>
          <Button className="btn btn-sm btn-danger" onClick={() => {setSearchTerm(""); getAllPosts();}}>Clear</Button>
        </div>
      </div>
    </div>
  )
}

export default PostSearch;