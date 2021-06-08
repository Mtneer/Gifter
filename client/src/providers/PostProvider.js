import React, { useState, useContext } from "react";
import { UserProfileContext } from "./UserProfileProvider";

export const PostContext = React.createContext();

export const PostProvider = (props) => {
  const apiUrl = "/api/post";
  // const { getToken } = useContext(UserProfileContext);
  
  const [posts, setPosts] = useState([]);
  const [ searchTerm, setSearchTerm ] = useState("");

  const getAllPosts = () => {
    return fetch(apiUrl)
      .then(resp => resp.json())
      .then(setPosts);
  };
  // const getAllPosts = () => {
  //   getToken().then((token) =>
  //       fetch(apiUrl, {
  //           method: "GET",
  //           headers: {
  //               Authorization: `Bearer ${token}`
  //           }
  //       }))
  //     .then(resp => resp.json())
  //     .then(setPosts);
  // };

  const addPost = (post) => {
    return fetch(apiUrl, {
            method: "POST",
            headers: {
                // Authorization: `Bearer ${token}`,
                "Content-Type": "application/json",
            },
            body: JSON.stringify(post),
    }).then(resp => {
      if (resp.ok) {
        return resp.json();
      }
      throw new Error("Unauthorized");
    });
  };
  // const addPost = (post) => {
  //   getToken().then((token) =>
  //       fetch(apiUrl, {
  //           method: "POST",
  //           headers: {
  //               Authorization: `Bearer ${token}`,
  //               "Content-Type": "application/json",
  //           },
  //           body: JSON.stringify(post),
  //   }).then(resp => {
  //     if (resp.ok) {
  //       return resp.json();
  //     }
  //     throw new Error("Unauthorized");
  //   }));
  // };

  const searchPost = () => {
    return fetch(`${apiUrl}/search?q=${searchTerm}&sortDesc=false`)
        .then((res) => res.json())
        .then(setPosts);
  };
  // const searchPost = () => {
  //   getToken().then((token) =>
  //       fetch(`${apiUrl}/search?q=${searchTerm}&sortDesc=false`, {
  //           method: "GET",
  //           headers: {
  //               Authorization: `Bearer ${token}`
  //           }
  //       }))
  //       .then((res) => res.json())
  //       .then(setPosts);
  // };

  return (
    <PostContext.Provider value={{ posts, getAllPosts, addPost, searchPost, setSearchTerm }}>
      {props.children}
    </PostContext.Provider>
  );
};
