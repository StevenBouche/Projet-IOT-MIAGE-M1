import React, { Component } from 'react';
import {Row, Container, Form, InputGroup} from 'react-bootstrap'
import {Line} from 'react-chartjs-2';
import DataService from '../Services/DataService'

const nbValues = [10, 20, 30 , 40, 50];

class Equipment extends Component {

  constructor(props) {
    super(props);
 
    this.state = {
      datas : [],
      equipment : props.obj,
      stateChart : {},
      nbData : 20, 
      configChart: {
        responsive: true,
        maintainAspectRatio: true,
        title:{
          display:true,
          text:'Data of equipment',
          fontSize:20
        },
        legend:{
          display:true,
          position:'right'
        }
      }
    }
  }

  async getData(){
    return await DataService.getLastData(this.state.equipment.equipmentId,this.state.nbData);

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

  async componentDidMount() {

    await this.pullData();

      this.interval = setInterval(async () => {
        await this.updateChart(this)
      }, 1000);
  }

  async pullData(){
      var datas = await this.getData();
      if(datas!==undefined&&datas.length>0){
        var stateChart = this.updateStateChart(datas);
        this.setState({stateChart: stateChart, datas: datas});
      }
  }

    updateStateChart(datas){

      var labels = [];
      var dataTemp = [];
      var dataLum = [];

      datas.forEach(data => {
          labels.push(this.timestampToDateString(data.timestamp))
          dataTemp.push(data.temperature);
          dataLum.push(data.ligth);
      });

      return { labels: labels, datasets: [ this.getDatasets("Temperature",dataTemp,'rgba(0,255,0,0.3)'), this.getDatasets("Ligth",dataLum,'rgba(75,192,192,1)') ] }

    }

    async componentWillUnmount() {
      clearInterval(this.interval);
    }

    async updateChart(component){

        var id = component.state.equipment.equipmentId;
        var lastTimestamp = component.state.datas[component.state.datas.length-1].timestamp;
        
        var data = await DataService.getLastDataUpdate(id,lastTimestamp);

        if(data!==undefined&&data.length>0){

          var datas = component.state.datas;

          data.forEach(d => {
              datas.push(d);
          })

          var stateChart = component.updateStateChart(datas);

          component.setState({stateChart: stateChart, datas: datas});

        }
      
    }

    getDatasets(label,data,bordercolor){
        return {
          label: label,
          fill: false,
          lineTension: 0.5,
          backgroundColor: 'rgba(75,192,192,1)',
          borderColor: bordercolor,
          borderWidth: 2,
          data: data
        };
    }

    async handleChange(e){
      var value = e.target.value;
      await this.setState({ nbData: value });
      await this.pullData();
    }

    render() {

      let id = "id"+this.state.equipment.Id;

      return <div className={id}>
        <Container style={{ margin: '1%' }}>
          <Row><h2>Equipment {this.state.equipment.equipmentId}</h2></Row>
          <Row>
            <InputGroup className="mb-3">
            <Form.Group controlId="exampleForm.ControlSelect1">
              <Form.Label>Nb data polling</Form.Label>
              <Form.Control as="select" value={this.state.nbData} onChange={this.handleChange.bind(this)}>
              {nbValues.map((eq, i) => {     
                        return (
                          <option key={i} value={eq} selected={eq === this.state.nbData}>{eq}</option>
                        )}
                    )}             
              </Form.Control>
            </Form.Group>

            </InputGroup>
          </Row>
          <Row style={{ margin: '1%' }}><Line data={this.state.stateChart} options={this.state.configChart}/></Row>
        </Container>
        
      </div>;
    }
}

export default Equipment;