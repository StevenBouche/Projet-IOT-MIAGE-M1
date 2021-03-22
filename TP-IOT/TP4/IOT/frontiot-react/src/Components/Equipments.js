import React, { Component } from 'react';
import {Button, Container, Card, Row} from 'react-bootstrap'
import EquipmentService from '../Services/EquipmentService.js';
import Equipment from './Equipment.js'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCheck, faTimes } from '@fortawesome/free-solid-svg-icons'



class Equipments extends Component {

    state = {
        equipments : [],
        obj : null
    }

    iconTrue = <FontAwesomeIcon icon={faCheck} color="green"/>;
    iconFalse = <FontAwesomeIcon icon={faTimes} color="red"/>;

    async getData(){
        var data = await EquipmentService.getEquipments();
        this.setState({equipments: data});
    }

    async componentDidMount() {
        await this.getData();
      }

    selectEquipment(id){
        this.setState({obj: id});
    }

    showEquipment(){    
        if(this.state.obj!==null){
            return (
                <Equipment obj={this.state.obj}></Equipment>
            );
        }
        return;
    }

    timestampToDateString(timestamp){
        var date = new Date(timestamp*1000);
        return date.getDate()+
        "/"+(date.getMonth()+1)+
        "/"+date.getFullYear()+
        " "+date.getHours()+
        ":"+date.getMinutes()+
        ":"+date.getSeconds();
    }

    iconOnline(bool){
        return bool ? this.iconTrue : this.iconFalse;
    }

    render() {
      return <div className="equipments"> 
        <Container fluid style={{ margin: '1%' }}>
                <Row><h2>Equipments</h2></Row>        
                <Row>
                    {this.state.equipments.map((eq, i) => {     
                        return (
                            <Card key={eq.Id} style={{ width: '20rem', margin: '1%'  }} >
                                <Card.Body>
                                <Card.Title>{eq.equipmentId}</Card.Title>
                                    <Container>
                                        <Row><strong>Adresse IP : </strong> {eq.adressIP}</Row>
                                        <Row><strong>Is Online : </strong>{this.iconOnline(eq.isOnline)}</Row>
                                        <Row><strong>Last connection : </strong>{this.timestampToDateString(eq.lastConnectionTimestamp)}</Row>
                                    </Container>
                                </Card.Body>
                                <Card.Footer>
                                    <Button variant="primary" onClick={(e) => this.selectEquipment(eq)}>Select</Button>
                                </Card.Footer>
                        </Card>
                        )}
                    )}
                </Row>
        </Container>

        {this.showEquipment()}
        
      </div>;
    }
}

export default Equipments;