import React, { useContext, useEffect, useState } from "react";
import { useHistory } from "react-router-dom";
import { PostContext } from "../providers/PostProvider";
import { Card, CardBody, Button, Form, FormGroup, Label, Input, FormText } from 'reactstrap';

export const PostForm = () => {
    const { addPost } = useContext(PostContext)

    /* Define the intial state of the form inputs with useState() */
    const [post, setPost] = useState({
      Title: "",
      ImageUrl: "",
      Caption: ""
    });

    const history = useHistory();

    //when a field changes, update state. The return will re-render and display based on the values in state
    //Controlled component
    const handleControlledInputChange = (event) => {
      /* When changing a state object or array,
      always create a copy, make changes, and then set state.*/
      const newPost = { ...post }
      /* Animal is an object with properties.
      Set the property to the new value
      using object bracket notation. */
      newPost[event.target.id] = event.target.value
      // update state
      setPost(newPost)
    }

    const handleClickSavePost = (event) => {
        event.preventDefault() //Prevents the browser from submitting the form

        //invoke addAnimal passing animal as an argument.
        //once complete, change the url and display the animal list
        addPost(post)
    }

    return (
        <div className="container">
            <div className = "row">
                <div className="col-lg-2"></div>
                <Card className="col-lg-8"> 
                    <CardBody>
                        <Form>
                            {/* <h2 className="postForm__title">New Post</h2> */}
                            <FormGroup className="form-group">
                                    <Label htmlFor="Title">Post Title:</Label>
                                    <Input type="text" id="Title" onChange={handleControlledInputChange} required autoFocus className="form-control" placeholder="Post Title" value={post.Title}/>
                            </FormGroup>
                            <FormGroup className="form-group">
                                    <Label htmlFor="ImageUrl">Image URL:</Label>
                                    <Input type="text" id="ImageUrl" onChange={handleControlledInputChange} required autoFocus className="form-control" placeholder="Image URL" value={post.ImageUrl}/>
                            </FormGroup>
                            <FormGroup className="form-group">
                                    <Label htmlFor="Caption">Post Caption:</Label>
                                    <Input type="text" id="Caption" onChange={handleControlledInputChange} required autoFocus className="form-control" placeholder="Caption" value={post.Caption}/>
                            </FormGroup>
                            <Button className="btn btn-secondary btn-sm"
                                onClick={handleClickSavePost}>
                                Save Post
                            </Button>
                        </Form>
                    </CardBody>
                </Card>
                <div className="col-lg-2"></div>
            </div>
        </div>
    )
}

export default PostForm;