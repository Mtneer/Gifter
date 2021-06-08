import React, { useState, useContext } from "react";
import { Link } from "react-router-dom";
import { UserProfileContext } from "../providers/UserProfileProvider";

const Header = () => {
  const { isLoggedIn, logout } = useContext(UserProfileContext);
  const [isOpen, setIsOpen] = useState(false);

  return (
    <nav className="navbar navbar-expand navbar-dark bg-info">
      { isLoggedIn &&
        <> 
          <Link to="/" className="navbar-brand">
            GiFTER
          </Link>
          <ul className="navbar-nav mr-auto">
            <li className="nav-item">
              <Link to="/" className="nav-link">
                Feed
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/posts/add" className="nav-link">
                New Post
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/login" className="nav-link" onClick={logout}>
                Logout
              </Link>
            </li>
          </ul>
        </>
    }
    { !isLoggedIn &&
      <>
        <ul className="navbar-nav mr-auto">
          <li className="nav-item">
            <Link to="/login" className="nav-link">
              Login
            </Link>
          </li>
          <li className="nav-item">
            <Link to="/register" className="nav-link">
              Register
            </Link>
          </li>
        </ul>
      </>
    }
    </nav>
  );
};

export default Header;
