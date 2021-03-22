import React, { Component } from 'react';
import {Navbar, } from 'react-bootstrap'
import Equipments from '../Components/Equipments.js';


class Dashboard extends Component {
    render() {
      return <div className="App">

        <Navbar bg="dark" variant="dark" expand="lg" className="">
          <Navbar.Brand href="#home">IOT Dashboard</Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
        </Navbar>

        <Equipments>

        </Equipments>

    </div>;
    }
}

export default Dashboard;